using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class HordeSpawner : MonoBehaviour
{
    [Header("기본 설정")]
    public GameObject monsterPrefab;
    public Transform player;

    [Header("스폰 규칙")]
    public int maxMonsters = 10;     // 유지할 최대 마릿수
    public int hordeCount = 3;       // 한 번에 소환할 무리 크기
    public float respawnTime = 3f;   // 체크 주기
    
    [Header("위치 설정 (플레이어 기준)")]
    public float minSpawnRadius = 15f; // ★ 최소 이만큼은 떨어져서 나와라 (코앞 스폰 방지)
    public float maxSpawnRadius = 30f; // ★ 최대 이 거리 안에는 나와라
    public float hordeSpread = 5f;     // 무리들이 뭉쳐있는 정도

    [Header("청소 설정 (중요)")]
    public float despawnDistance = 50f; // ★ 플레이어랑 이보다 멀어지면 삭제 (재소환을 위해)

    [Header("안전 장치")]
    public LayerMask avoidLayers; // 장애물 레이어 (벽 등)
    public float checkRadius = 1.0f;

    private List<GameObject> activeMonsters = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 1. null(죽은 몬스터) 정리
            activeMonsters.RemoveAll(x => x == null);

            // 2. ★ 멀리 있는 몬스터 삭제 (청소)
            // 이걸 해야 플레이어가 이동했을 때 내 주변에 새 몬스터가 나옵니다.
            DespawnFarMonsters();

            // 3. 모자란 만큼 소환
            if (activeMonsters.Count < maxMonsters)
            {
                SpawnHorde();
            }

            yield return new WaitForSeconds(respawnTime);
        }
    }

    // ★ 플레이어와 너무 멀어진 몬스터 제거 함수
    void DespawnFarMonsters()
    {
        if (player == null) return;

        // 리스트를 거꾸로 돌면서 삭제 (리스트 안전 삭제 방식)
        for (int i = activeMonsters.Count - 1; i >= 0; i--)
        {
            GameObject mon = activeMonsters[i];
            if (mon != null)
            {
                float dist = Vector3.Distance(player.position, mon.transform.position);
                if (dist > despawnDistance)
                {
                    Destroy(mon);
                    activeMonsters.RemoveAt(i);
                }
            }
        }
    }

    public void SpawnHorde()
    {
        if (player == null) return;

        Vector3 hordeCenter;
        
        // ★ 플레이어 주변 도넛 모양 범위에서 위치 찾기
        if (GetRandomPositionAroundPlayer(out hordeCenter))
        {
            for (int i = 0; i < hordeCount; i++)
            {
                if (activeMonsters.Count >= maxMonsters) break;

                // 무리 중심점에서 약간 흩뿌리기
                Vector3 spawnPos;
                if (GetRandomNavMeshPosition(hordeCenter, hordeSpread, out spawnPos))
                {
                    // 벽 속에 생기는지 체크
                    if (!Physics.CheckSphere(spawnPos, checkRadius, avoidLayers))
                    {
                        GameObject newMonster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
                        activeMonsters.Add(newMonster);

                        // 타겟 설정
                        EnemyFollowRB monsterScript = newMonster.GetComponent<EnemyFollowRB>();
                        if (monsterScript != null)
                        {
                            monsterScript.target = this.player;
                        }
                    }
                }
            }
        }
    }

    // ★ 플레이어 주변 도넛 모양(최소~최대 반경 사이) 좌표 구하기
    bool GetRandomPositionAroundPlayer(out Vector3 result)
    {
        for (int i = 0; i < 30; i++) // 30번 시도
        {
            // 1. 랜덤한 방향(각도) 구하기
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            
            // 2. 최소~최대 거리 사이의 랜덤 거리 구하기
            float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);

            // 3. 플레이어 위치에 더하기 (높이는 플레이어와 같게)
            Vector3 targetPos = player.position + new Vector3(randomCircle.x, 0, randomCircle.y) * randomDistance;

            // 4. 그 위치가 NavMesh 위인지 확인
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, 5.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    // 일반적인 NavMesh 위 랜덤 위치 (무리 흩뿌리기용)
    bool GetRandomNavMeshPosition(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 2.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            // 최소 거리 (빨간색 - 이 안에는 안 나옴)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, minSpawnRadius);

            // 최대 거리 (초록색 - 이 사이에서 나옴)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, maxSpawnRadius);

            // 삭제 거리 (회색 - 여기 넘어가면 삭제)
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(player.position, despawnDistance);
        }
    }
}
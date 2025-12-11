using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("총알 설정")]
    public float speed = 50f;   // 총알 속도
    public float damage = 10f;  // 데미지
    public float lifeTime = 3f; // 수명 (3초 뒤 삭제)

    void Start()
    {
        // 시작하자마자 앞으로 날아가도록 속도 설정
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Unity 6버전 이상: linearVelocity
            // Unity 2022 이하 구버전: velocity
            rb.linearVelocity = transform.forward * speed; 
        }

        // 일정 시간이 지나면 자동으로 삭제 (성능 관리)
        Destroy(gameObject, lifeTime);
    }

    // 물체와 충돌했을 때 실행되는 함수 (Is Trigger 체크 필수)
    void OnTriggerEnter(Collider other)
    {
        // ★ [핵심] 부딪힌 물체의 레이어가 "NoSpawn"이면 무시! (투명벽 통과)
        if (other.gameObject.layer == LayerMask.NameToLayer("SafeZone"))
        {
            return; // 아무 일도 하지 않고 함수 종료 (Destroy 안 됨)
        }

        // 플레이어 몸에 맞았을 때도 무시 (자해 방지)
        if (other.CompareTag("Player")) 
        {
            return; 
        }

        // 보스 몬스터 피격 판정
        BossHealth boss = other.GetComponent<BossHealth>();
        // 혹시 콜라이더가 자식에 있을 수 있으니 부모에서도 찾음
        if (boss == null) 
        {
            boss = other.GetComponentInParent<BossHealth>();
        }

        if (boss != null)
        {
            boss.TakeDamage(damage); // 보스 체력 깎기
            // Debug.Log("보스 명중!");
        }

        // 보스든 벽이든, "NoSpawn"과 "Player"가 아닌 것에 맞았으면 총알 삭제
        Destroy(gameObject);
    }
}
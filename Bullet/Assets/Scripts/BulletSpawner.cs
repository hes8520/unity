using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    // 생성할 탄알 프리팹
    public GameObject bulletPrefab;

    // 발사 주기 (1.0f = 1초)
    public float fireRate = 1.0f;

    // 다음 발사 시간 계산용 변수
    private float nextFireTime = 0f;

    // 플레이어의 Transform (위치를 추적하기 위함)
    private Transform playerTransform;

    void Start()
    {
        // "Player" 태그를 가진 오브젝트를 찾아서 playerTransform 변수에 할당
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("BulletSpawner: 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        // 플레이어가 존재하고, 현재 시간이 다음 발사 시간보다 크거나 같으면 발사
        if (playerTransform != null && Time.time >= nextFireTime)
        {
            Fire();
            // 다음 발사 시간을 현재 시간 + 발사 주기로 설정
            nextFireTime = Time.time + fireRate;
        }
    }

    void Fire()
    {
        // 1. 방향 계산: 플레이어 위치 - 현재(스포너) 위치 (3D에서도 동일)
        Vector3 direction = playerTransform.position - transform.position;

        // 2. 3D 회전값 계산: 
        // 해당 방향(direction)을 바라보는 회전값(Quaternion)을 생성합니다.
        Quaternion rotation = Quaternion.LookRotation(direction);

        // 3. 탄알 생성: 계산된 위치와 3D 회전값으로 탄알 프리팹을 생성
        Instantiate(bulletPrefab, transform.position, rotation);
    }
}
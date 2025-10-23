using UnityEngine;

// 3D Rigidbody가 없으면 자동으로 추가해주는 기능
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    // 탄알 이동 속도 (Inspector에서 0이 아닌 값으로 설정!)
    public float speed = 10f;

    // 3D Rigidbody 컴포넌트
    private Rigidbody rb;

    void Start()
    {
        // 3D Rigidbody 컴포넌트를 가져옵니다.
        rb = GetComponent<Rigidbody>();
        
        // 탄알이 바라보는 방향(transform.forward)으로 속도를 줍니다.
        // (Spawner의 LookRotation이 Z축(forward)을 플레이어 방향으로 정렬했기 때문)
        rb.velocity = transform.forward * speed;
    }

    // 3D Collider와 '충돌'했을 때 호출되는 함수 (Collision2D 아님)
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 오브젝트의 태그가 "Wall"인지 확인
        if (collision.gameObject.CompareTag("Wall"))
        {
            // "Wall" 태그가 맞다면 이 탄알 오브젝트를 파괴
            Destroy(gameObject);
        }
    }
}
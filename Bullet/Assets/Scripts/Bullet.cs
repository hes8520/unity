using UnityEngine;

// 3D Rigidbody가 없으면 자동으로 추가해주는 기능
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    // 탄알 이동 속도 (Inspector에서 0이 아닌 값으로 설정!)
    public float speed = 10f;

    public float lifeTime = 3.0f; // 3초 뒤에 사라지도록 설정

    // 3D Rigidbody 컴포넌트
    private Rigidbody rb;

    void Start()
    {
        // 3D Rigidbody 컴포넌트를 가져옵니다.
        rb = GetComponent<Rigidbody>();

        // 탄알이 바라보는 방향(transform.forward)으로 속도를 줍니다.
        // (Spawner의 LookRotation이 Z축(forward)을 플레이어 방향으로 정렬했기 때문)
        rb.velocity = transform.forward * speed;

        Destroy(gameObject, lifeTime);
    }

    // ★★ 수정된 부분 ★★
    // OnTriggerEnter 함수를 Bullet 클래스의 괄호 안으로 이동시켰습니다.
    void OnTriggerEnter(Collider other)
    {
        // 충돌한 상대방 게임 오브젝트가 Player 태그를 가진 경우
        if (other.tag == "Player")
        {
            // 상대방 게임 오브젝트에서 PlayerController 컴포넌트를 가져오기
            PlayerController playerController = other.GetComponent<PlayerController>();

            // 상대방으로부터 PlayerController 컴포넌트를 가져오는데 성공했다면
            if (playerController != null)
            {
                // 상대방 PlayerController 컴포넌트의 Die() 메서드 실행
                playerController.Die();
            }
        }
    }
    
} // <--- Bullet 클래스가 여기서 끝납니다.
// ★★ 맨 마지막에 있던 불필요한 괄호(})를 제거했습니다. ★★
using UnityEngine;

// 이 스크립트가 작동하려면 Rigidbody 컴포넌트가 필요하다고 명시합니다.
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement_Rigidbody : MonoBehaviour
{
    [Header("움직임 설정")]
    public float moveSpeed = 5f; // 이동 속도
    public float rotateSpeed = 720f; // 회전 속도 (초당 720도)

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Camera mainCamera;

    void Start()
    {
        // Rigidbody 컴포넌트를 가져와서 변수에 저장합니다.
        rb = GetComponent<Rigidbody>();
        
        // 물리 엔진에 의해 캐릭터가 넘어지지 않도록 X, Z축 회전을 고정합니다.
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // 메인 카메라를 찾아서 변수에 저장합니다. (성능을 위해 Start에서 한 번만 호출)
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 1. 입력 감지 (Update에서 매 프레임 감지)
        float moveX = Input.GetAxis("Horizontal"); // A, D 또는 화살표 좌우
        float moveZ = Input.GetAxis("Vertical");   // W, S 또는 화살표 상하

        // 2. 카메라 기준의 이동 방향 계산
        // 카메라가 바라보는 정면 방향
        Vector3 cameraForward = mainCamera.transform.forward;
        // 카메라의 오른쪽 방향
        Vector3 cameraRight = mainCamera.transform.right;

        // 카메라의 Y축 회전값은 무시하여, 캐릭터가 위아래로 기울어지지 않게 합니다.
        cameraForward.y = 0;
        cameraRight.y = 0;

        // 카메라 방향 기준으로 실제 이동할 방향을 계산합니다.
        // (W/S * 카메라 정면) + (A/D * 카메라 오른쪽)
        moveDirection = (cameraForward.normalized * moveZ + cameraRight.normalized * moveX).normalized;
    }

    void FixedUpdate()
    {
        // 3. 물리 이동 (FixedUpdate에서 고정된 주기로 실행)
        // 계산된 방향으로 캐릭터를 이동시킵니다.
        // rb.position: 현재 위치
        // moveDirection * moveSpeed * Time.fixedDeltaTime: 이번 프레임에 이동할 거리
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

        // 4. 자연스러운 회전
        // 캐릭터가 움직이는 방향(moveDirection)을 바라보도록 합니다.
        if (moveDirection != Vector3.zero) 
{
            // --- 수정된 부분 ---
             // 이동 방향(moveDirection)에서 Y축으로 90도 회전한 방향을 '앞'으로 설정합니다.
            Vector3 targetForwardDirection = Quaternion.Euler(0, 90, 0) * moveDirection;
    
            // 바라볼 목표 회전값을 계산합니다.
            Quaternion targetRotation = Quaternion.LookRotation(targetForwardDirection);
            // --- 여기까지 ---

            // (참고: 만약 반대로 돈다면 90 대신 -90을 넣으세요.)
    
            // 현재 회전값에서 목표 회전값으로 부드럽게 회전합니다.
            Quaternion newRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);
    
            rb.MoveRotation(newRotation);
        }
    }
}
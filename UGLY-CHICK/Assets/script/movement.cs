using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;                 // 이동 속도 (m/s)
    public bool cameraRelative = true;           // 카메라 기준 이동

    [Header("Jump & Ground")]
    public float jumpForce = 5f;                 // 점프 임펄스
    public LayerMask groundMask;                 // 지면 레이어
    public float groundCheckRadius = 0.2f;       // 지면 체크 반지름
    public Vector3 groundCheckOffset = new Vector3(0f, -0.9f, 0f);

    Rigidbody rb;
    Transform cam;

    Vector3 moveInput;
    bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // 물리 회전 방지 (회전은 코드로 처리)
        cam = Camera.main ? Camera.main.transform : null;
    }

    void Update()
    {
        // 입력 수집
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 raw = new Vector3(x, 0f, z);
        raw = raw.normalized;

        if (cameraRelative && cam != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            moveInput = forward * raw.z + right * raw.x;
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
        }
        else
        {
            moveInput = raw;
        }

        // 점프 입력 (업데이트에서 입력 감지)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        // 지면 체크 (OverlapSphere 사용 — 더 안정적)
        Vector3 checkPos = transform.position + groundCheckOffset;
        isGrounded = Physics.CheckSphere(checkPos, groundCheckRadius, groundMask);

        // 수평 이동은 MovePosition으로 처리 (Rigidbody 물리와 충돌을 잘 처리)
        Vector3 horizontal = new Vector3(moveInput.x, 0f, moveInput.z) * moveSpeed * Time.fixedDeltaTime;
        Vector3 targetPos = rb.position + horizontal;
        rb.MovePosition(targetPos);

        // 회전은 MoveRotation으로 부드럽게 (입력 방향 그대로 사용)
        if (moveInput.sqrMagnitude > 0.001f)
        {
            Vector3 lookDir = new Vector3(moveInput.x, 0f, moveInput.z);
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 12f * Time.fixedDeltaTime));
        }
    }

    void OnDrawGizmosSelected()
    {
        // 클래스 내부에 있어야 하고, groundCheckOffset/groundCheckRadius가 같은 클래스 멤버여야 함
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + groundCheckOffset, groundCheckRadius);
    }
}

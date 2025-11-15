using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;
    public bool cameraRelative = true;

    [Header("Jump & Ground")]
    public float jumpUpForce = 5f;        // 위로 튀는 힘
    public float jumpForwardForce = 6f;   // 앞으로 튀는 힘
    public LayerMask groundMask;
    public float groundCheckRadius = 0.2f;
    public Vector3 groundCheckOffset = new Vector3(0f, -0.9f, 0f);

    // --- ★ 애니메이션 및 물리 변수 선언 ---
    Rigidbody rb;
    Transform cam;
    Animator anim; // 애니메이터 컴포넌트
    
    Vector3 moveInput;
    bool isGrounded;
    bool jumpRequested = false; // 점프 요청 플래그 (물리 버그 수정용)

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        cam = Camera.main ? Camera.main.transform : null;

        // --- ★ 애니메이터 컴포넌트 가져오기 ---
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 raw = new Vector3(x, 0f, z).normalized;

        if (cameraRelative && cam != null)
        {
            Vector3 camForward = cam.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 camRight = cam.right;
            camRight.y = 0;
            camRight.Normalize();

            moveInput = camForward * raw.z + camRight * raw.x;
        }
        else
        {
            moveInput = raw;
        }

        // --- ★ "Super Jump" 버그 수정을 위해 Jump() 직접 호출 대신 '요청'만 함 ---
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }

        // --- ★ "IsMoving" 걷기 애니메이션 파라미터 업데이트 ---
        if (anim != null)
        {
            anim.SetBool("IsWalking", moveInput.sqrMagnitude > 0.001f);
        }
    }

    void Jump()
    {
        // --- ★ "JumpTrigger" 점프 애니메이션 파라미터 실행 ---
        if (anim != null)
        {
            anim.SetTrigger("JumpTrigger");
        }

        // 모델 정면 = transform.forward (월드 기준) + 90도 보정
        Vector3 modelForward = (transform.rotation * Quaternion.Euler(0, -90f, 0)) * Vector3.forward;

        Vector3 force =
            modelForward * jumpForwardForce   // 앞 방향
            + Vector3.up * jumpUpForce;       // 위 방향

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // 기존 위속 제거
        rb.AddForce(force, ForceMode.Impulse);
    }

    void FixedUpdate()
    {
        // 지면 체크
        Vector3 checkPos = transform.position + groundCheckOffset;
        isGrounded = Physics.CheckSphere(checkPos, groundCheckRadius, groundMask);

        // --- ★ "IsGrounded" 땅 착지/공중 파라미터 업데이트 ---
        if (anim != null)
        {
            anim.SetBool("IsGrounded", isGrounded);
        }

        // --- ★ 점프 요청 처리 (FixedUpdate에서 물리 실행) ---
        if (jumpRequested)
        {
            Jump();
            jumpRequested = false;
            isGrounded = false; // 점프하는 순간 '땅에 닿지 않음'으로 강제
        }

        // 이동 (지상일 때만)
        if (isGrounded)
        {
            Vector3 horizontal = moveInput * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + horizontal);
        }

        // 회전
        if (moveInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveInput);

            // 모델이 90도 틀어져 있음
            targetRot *= Quaternion.Euler(0, 90f, 0);

            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 12f * Time.fixedDeltaTime));
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + groundCheckOffset, groundCheckRadius);
    }
}
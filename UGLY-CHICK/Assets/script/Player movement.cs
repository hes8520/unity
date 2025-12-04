using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 필수

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;
    public bool cameraRelative = true;

    [Header("Roll (Dodge)")]
    public float rollSpeed = 10f;      // 구르기 속도
    public float rollDuration = 0.8f;  // 구르기 지속 시간 (애니메이션 길이와 비슷하게)
    public float rollCooldown = 1.0f;  // 구르기 쿨타임
    
    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundCheckRadius = 0.2f;
    public Vector3 groundCheckOffset = new Vector3(0f, -0.9f, 0f);

    // --- ★ 상태 변수 (외부에서 접근 가능) ---
    public bool isInvincible = false; // 무적 상태 확인용 (다른 스크립트에서 참조)

    // 내부 변수
    Rigidbody rb;
    Transform cam;
    Animator anim;
    
    Vector3 moveInput;
    bool isGrounded;
    
    // 구르기 관련 내부 변수
    bool isRolling = false;
    float lastRollTime = -99f; // 마지막 구르기 시간
    Vector3 rollDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        cam = Camera.main ? Camera.main.transform : null;
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 1. 구르기 중에는 방향 전환 입력을 받지 않음 (방향 고정)
        if (isRolling) return;

        // 2. 이동 입력 받기
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

        // 3. 걷기 애니메이션
        if (anim != null)
        {
            anim.SetBool("IsWalking", moveInput.sqrMagnitude > 0.001f);
        }

        // 4. 구르기 입력 (Spacebar = Jump 키 사용)
        // 땅에 있고 + 쿨타임 지났고 + 구르는 중이 아닐 때
        if (Input.GetButtonDown("Jump") && isGrounded && !isRolling && Time.time >= lastRollTime + rollCooldown)
        {
            StartCoroutine(Roll());
        }
    }

    void FixedUpdate()
    {
        // 지면 체크
        Vector3 checkPos = transform.position + groundCheckOffset;
        isGrounded = Physics.CheckSphere(checkPos, groundCheckRadius, groundMask);

        if (anim != null)
        {
            anim.SetBool("IsGrounded", isGrounded);
        }

        // 구르기 중일 때는 이동 로직을 코루틴(Roll)에게 맡김
        if (isRolling) return;

        // 일반 이동 (지상일 때만)
        if (isGrounded)
        {
            Vector3 targetVelocity = moveInput * moveSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        }

        // 회전 (이동 중일 때)
        if (moveInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveInput);
            targetRot *= Quaternion.Euler(0, 90f, 0); // 모델 보정
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 12f * Time.fixedDeltaTime));
        }
    }

    // --- ★ 구르기 코루틴 (핵심 로직) ---
    IEnumerator Roll()
    {
        isRolling = true;
        isInvincible = true; // ★ 무적 시작
        lastRollTime = Time.time;

        // 1. 구를 방향 결정
        // 이동 중이면 이동 방향으로, 멈춰있으면 캐릭터가 보는 방향(모델 보정 고려)으로 구름
        if (moveInput.sqrMagnitude > 0.001f)
        {
            rollDirection = moveInput;
        }
        else
        {
            // 모델이 90도 돌아가 있으므로, forward 기준 왼쪽(-90)이 실제 정면
             rollDirection = (transform.rotation * Quaternion.Euler(0, -90f, 0)) * Vector3.forward;
        }

        // 2. 애니메이션 실행
        if (anim != null) anim.SetTrigger("Roll");

        // 3. 구르기 동작 (지속 시간 동안)
        float currentRollTime = 0f;
        while (currentRollTime < rollDuration)
        {
            // 강제로 구르기 방향으로 속도 적용
            rb.linearVelocity = new Vector3(rollDirection.x * rollSpeed, rb.linearVelocity.y, rollDirection.z * rollSpeed);
            
            // 회전도 구르는 방향을 보게 함
            Quaternion targetRot = Quaternion.LookRotation(rollDirection);
            targetRot *= Quaternion.Euler(0, 90f, 0);
            rb.MoveRotation(targetRot);

            currentRollTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 4. 구르기 종료
        isInvincible = false; // ★ 무적 해제
        isRolling = false;
        
        // 끝나면 속도를 줄여줌 (미끄러짐 방지)
        rb.linearVelocity = Vector3.zero; 
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + groundCheckOffset, groundCheckRadius);
    }
}
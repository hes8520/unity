using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;
    public bool cameraRelative = true;

    [Header("Roll (Dodge)")]
    public float rollSpeed = 10f;
    public float rollDuration = 0.8f;
    public float rollCooldown = 1.0f;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundCheckRadius = 0.2f;
    public Vector3 groundCheckOffset = new Vector3(0f, -0.9f, 0f);

    // --- 상태 변수 ---
    public bool isInvincible = false;

    // 내부 변수
    Rigidbody rb;
    Transform cam;
    Animator anim;

    Vector3 moveInput;
    bool isGrounded;
    bool isRolling = false;
    float lastRollTime = -99f;
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
        // 구르는 중이면 입력 무시
        if (isRolling) return;

        // -------------------------
        // WASD 이동 입력만 받음
        // 방향키는 퀵슬롯용으로 사용 가능
        // -------------------------
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.W)) z += 1f;
        if (Input.GetKey(KeyCode.S)) z -= 1f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;

        Vector3 raw = new Vector3(x, 0f, z).normalized;

        // 카메라 기준 방향 변환
        if (cameraRelative && cam != null)
        {
            Vector3 camForward = cam.forward; camForward.y = 0; camForward.Normalize();
            Vector3 camRight = cam.right; camRight.y = 0; camRight.Normalize();
            moveInput = camForward * raw.z + camRight * raw.x;
        }
        else moveInput = raw;

        // 걷기 애니메이션
        if (anim != null)
            anim.SetBool("IsWalking", moveInput.sqrMagnitude > 0.001f);

        // 구르기 입력
        if (Input.GetButtonDown("Jump") && isGrounded && !isRolling &&
            Time.time >= lastRollTime + rollCooldown)
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
            anim.SetBool("IsGrounded", isGrounded);

        if (isRolling) return;

        // 이동
        if (isGrounded)
        {
            Vector3 targetVelocity = moveInput * moveSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        }

        // 회전
        if (moveInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveInput);
            targetRot *= Quaternion.Euler(0, 90f, 0); // 모델 방향 보정
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 12f * Time.fixedDeltaTime));
        }
    }

    IEnumerator Roll()
    {
        isRolling = true;
        isInvincible = true;
        lastRollTime = Time.time;

        // 이동 방향 or 정면
        if (moveInput.sqrMagnitude > 0.001f)
            rollDirection = moveInput;
        else
            rollDirection = (transform.rotation * Quaternion.Euler(0, -90f, 0)) * Vector3.forward;

        if (anim != null)
            anim.SetTrigger("Roll");

        float currentRollTime = 0f;

        while (currentRollTime < rollDuration)
        {
            rb.linearVelocity = new Vector3(
                rollDirection.x * rollSpeed,
                rb.linearVelocity.y,
                rollDirection.z * rollSpeed
            );

            Quaternion targetRot = Quaternion.LookRotation(rollDirection);
            targetRot *= Quaternion.Euler(0, 90f, 0);
            rb.MoveRotation(targetRot);

            currentRollTime += Time.deltaTime;
            yield return null;
        }

        isInvincible = false;
        isRolling = false;
        rb.linearVelocity = Vector3.zero;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + groundCheckOffset, groundCheckRadius);
    }
}

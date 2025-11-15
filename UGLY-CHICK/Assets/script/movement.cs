using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;
    public bool cameraRelative = true;

    [Header("Jump & Ground")]
    public float jumpForce = 5f;
    public LayerMask groundMask;
    public float groundCheckRadius = 0.2f;
    public Vector3 groundCheckOffset = new Vector3(0f, -0.9f, 0f);

    Rigidbody rb;
    Transform cam;

    Vector3 moveInput;
    bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        cam = Camera.main ? Camera.main.transform : null;
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
    }

    void FixedUpdate()
    {
        Vector3 checkPos = transform.position + groundCheckOffset;
        isGrounded = Physics.CheckSphere(checkPos, groundCheckRadius, groundMask);

        // 이동
        Vector3 horizontal = moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + horizontal);

        // 회전 (여기에 보정 추가!!)
        if (moveInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveInput);

            // 🔥 모델이 90도 틀어져 있으므로 보정
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

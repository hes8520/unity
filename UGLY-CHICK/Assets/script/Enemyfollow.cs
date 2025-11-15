using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyFollowRB : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 4f;
    public float rotateSpeed = 8f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (target == null) return;

        // 방향 계산
        Vector3 dir = target.position - transform.position;
        dir.y = 0;   // 위아래 무시
        dir.Normalize();

        // 회전
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);

            // 🔥 플레이어처럼 모델이 90도 틀어져 있으면 보정
            targetRot *= Quaternion.Euler(0, 90f, 0);

            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime));
        }

        // 전진 이동
        Vector3 move = dir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }
}

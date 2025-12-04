using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyFollowRB : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 4f;
    public float rotateSpeed = 8f;
    
    [Header("Combat Settings")]
    public float attackRange = 2.0f;     // 공격 사거리
    public float detectionRange = 10.0f; // 감지 범위

    [Header("Physics Settings")]
    [Tooltip("기본값은 1. 숫자가 클수록 바닥에 강하게 붙습니다.")]
    public float gravityScale = 5.0f; // 🔥 중력 배율 추가 (기본 5배)

    Rigidbody rb;
    Animator anim;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // 🔥 중요: 유니티 기본 중력을 끕니다.
        // (우리가 아래에서 직접 더 센 중력을 적용할 것이기 때문입니다)
        rb.useGravity = false; 

        anim = GetComponentInChildren<Animator>(); 
        if (anim == null) anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        // 🔥 [중력 적용 로직]
        // Physics.gravity(기본 중력 -9.81) * 배율(5.0) 만큼 힘을 가함
        rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);

        if (target == null) return;

        // 1. 거리 및 방향 계산
        float distance = Vector3.Distance(transform.position, target.position);
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        dir.Normalize();

        // 2. 상태 분기 (거리 기준)
        if (distance > detectionRange)
        {
            // [상태 1: 감지 전 - 대기]
            if (anim != null)
            {
                anim.SetBool("IsMove", false);
                anim.SetBool("IsAttack", false);
            }
        }
        else if (distance > attackRange)
        {
            // [상태 2: 감지 됨 - 추적]
            LookAtTarget(dir);

            Vector3 move = dir * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);

            if (anim != null)
            {
                anim.SetBool("IsMove", true);
                anim.SetBool("IsAttack", false);
            }
        }
        else
        {
            // [상태 3: 공격 사거리 - 공격]
            LookAtTarget(dir);

            if (anim != null)
            {
                anim.SetBool("IsMove", false);
                anim.SetBool("IsAttack", true);
            }
        }
    }

    // 회전 로직
    void LookAtTarget(Vector3 dir)
    {
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            // 🔥 모델 90도 틀어짐 보정 유지
            targetRot *= Quaternion.Euler(0, 90f, 0);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime));
        }
    }
    
    // 기즈모 그리기
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
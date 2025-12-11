using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyFollowRB : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Move Settings")]
    public float moveSpeed = 4f;
    public float rotateSpeed = 8f;

    [Header("Combat Settings")]
    public float attackRange = 2.0f;
    public float detectionRange = 10.0f;

    [Header("Physics Settings")]
    [Tooltip("유니티 기본 중력에 곱해지는 배율. 숫자가 클수록 바닥에 더 강하게 붙습니다.")]
    public float gravityScale = 5.0f;

    Rigidbody rb;
    Animator anim;

    // ★ BGM용 전투 여부
    private bool isAggroed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // ★ 유니티 기본 중력 OFF (직접 중력 적용하게 하기 위함)
        rb.useGravity = false;

        anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponent<Animator>();
    }

    void OnDestroy()
    {
        // ★ 몬스터가 사라질 때 전투 해제
        if (isAggroed && BGMManager.Instance != null)
        {
            BGMManager.Instance.RemoveEnemyAggro();
        }
    }

    void FixedUpdate()
    {
        // ★ 강한 중력 적용
        rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);

        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        // 방향 계산
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        dir.Normalize();

        // ★ BGM Aggro 상태 체크
        CheckAggroState(distance);

        // 상태 분기
        if (distance > detectionRange)
        {
            // [대기 상태]
            if (anim != null)
            {
                anim.SetBool("IsMove", false);
                anim.SetBool("IsAttack", false);
            }
        }
        else if (distance > attackRange)
        {
            // [추적 상태]
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
            // [공격 상태]
            LookAtTarget(dir);

            if (anim != null)
            {
                anim.SetBool("IsMove", false);
                anim.SetBool("IsAttack", true);
            }
        }
    }

    // ★ BGM 매니저와 연동
    void CheckAggroState(float distance)
    {
        // 감지범위 안 → 전투 시작
        if (distance <= detectionRange && !isAggroed)
        {
            isAggroed = true;
            if (BGMManager.Instance != null) BGMManager.Instance.AddEnemyAggro();
        }
        // 감지범위 밖 → 전투 종료
        else if (distance > detectionRange && isAggroed)
        {
            isAggroed = false;
            if (BGMManager.Instance != null) BGMManager.Instance.RemoveEnemyAggro();
        }
    }

    // ★ 회전 로직(모델 90도 보정 포함)
    void LookAtTarget(Vector3 dir)
    {
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            targetRot *= Quaternion.Euler(0, 90f, 0); // 모델이 90도 틀어져 있을 때 보정
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime));
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

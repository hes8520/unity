using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class BossAI : MonoBehaviour
{
    [Header("--- ★ 시간 설정 (초 단위) ---")]
    [Header("[1. 쪼기 시간 (총 4.1초)]")]
    public float peckTotalTime = 4.1f;    // 전체 길이
    public float peckWindupTime = 1.88f;  // 공격 발생 전 대기 시간 (약 46프레임 지점)
    public float peckActiveTime = 1.23f;  // 공격 판정이 유지되는 시간 (약 30프레임 동안)

    [Header("[2. 돌진 시간 (총 2.08초)]")]
    public float dashTotalTime = 2.08f;   // 전체 길이
    public float dashWindupTime = 0.28f;  // 출발 전 준비 시간 (약 8프레임 지점)
    public float dashMoveTime = 1.44f;    // 실제 돌진하는 시간 (약 40프레임 동안)

    [Header("[3. 점프 시간 (총 2.08초)]")]
    public float jumpTotalTime = 2.08f;   // 전체 길이
    public float jumpWindupTime = 1.07f;  // 공중에 뜨기 전 준비 시간 (약 30프레임 지점)
    public float jumpAirTime = 0.4f;      // 공중에 떠 있는 시간 (약 11프레임 동안)

    [Header("--- 회전 보정 ---")]
    public float modelRotationOffset = 0f; 

    [Header("--- 공격 범위 조절 ---")]
    [Range(0, 360)] public float peckAngle = 40f; 
    public float dashWidth = 2.0f; 
    public float jumpRadius = 5.0f; 

    [Header("기본 설정")]
    public Transform player;
    public float moveSpeed = 3.5f;
    public float rotSpeed = 10f;
    public float detectionRange = 15f;
    public float attackRange = 3.0f;
    public float attackCooldown = 3.0f;

    [Header("데미지 설정")]
    public float peckDamage = 10f;
    public float dashDamage = 20f;
    public float jumpDamage = 30f;

    [Header("패턴 세부 설정")]
    public float dashSpeed = 15f;

    [Header("사운드 & 이펙트 & 장판")]
    public AudioClip peckSound;
    public AudioClip dashStartSound;
    public AudioClip dashHitSound;
    public AudioClip jumpUpSound;
    public AudioClip jumpLandSound;

    public GameObject peckHitEffect;
    public GameObject dashTrailEffect; 
    public GameObject dashHitEffect;
    public GameObject jumpStartEffect;
    public GameObject jumpLandEffect;

    public GameObject peckIndicator; 
    public GameObject dashIndicator; 
    public GameObject jumpIndicator; 

    private Animator anim;
    private Rigidbody rb;
    private AudioSource audioSource;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (isAttacking || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        
        Vector3 dirToPlayer = GetDirToPlayer();

        if (dirToPlayer != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dirToPlayer);
            Quaternion targetRot = lookRot * Quaternion.Euler(0, modelRotationOffset, 0);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotSpeed * Time.fixedDeltaTime));
        }

        if (dist <= attackRange)
        {
            anim.SetBool("IsMove", false);
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(ChooseAttackPattern());
            }
        }
        else if (dist <= detectionRange)
        {
            anim.SetBool("IsMove", true);
            Vector3 movePos = transform.position + dirToPlayer * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(movePos);
        }
        else
        {
            anim.SetBool("IsMove", false);
        }
    }

    Vector3 GetDirToPlayer()
    {
        if (player == null) return transform.forward;
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        return dir;
    }

    void LookAtPlayerInstant()
    {
        if (player == null) return;
        Vector3 dir = GetDirToPlayer();
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = lookRot * Quaternion.Euler(0, modelRotationOffset, 0);
        }
    }

    IEnumerator ChooseAttackPattern()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        rb.linearVelocity = Vector3.zero;

        int pattern = Random.Range(0, 3); 

        switch (pattern)
        {
            case 0: yield return StartCoroutine(Pattern_Peck()); break;
            case 1: yield return StartCoroutine(Pattern_Dash()); break;
            case 2: yield return StartCoroutine(Pattern_JumpSlam()); break;
        }

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    // --- 패턴 1: 쪼기 (총 4.1초) ---
    IEnumerator Pattern_Peck()
    {
        // 1. 애니메이션 시작
        anim.SetTrigger("AttackNormal");
        PlaySound(peckSound);

        // 2. 장판 깔기
        Vector3 attackDir = GetDirToPlayer(); 
        GameObject indicator = null;
        if (peckIndicator != null)
        {
            Quaternion indicatorRot = Quaternion.LookRotation(attackDir);
            Vector3 spawnPos = transform.position + attackDir * (attackRange / 2);
            indicator = Instantiate(peckIndicator, spawnPos, indicatorRot);
            
            Vector3 pos = indicator.transform.position; pos.y = 0.1f; indicator.transform.position = pos;
            
            float originalY = indicator.transform.localScale.y;
            indicator.transform.localScale = new Vector3(attackRange, originalY, attackRange); 
        }

        // 3. 기 모으기 (설정된 시간만큼 대기)
        yield return new WaitForSeconds(peckWindupTime); 

        // 4. 타격 직전 재조준
        LookAtPlayerInstant();
        attackDir = GetDirToPlayer(); 

        if (indicator != null) Destroy(indicator); 

        // 5. 타격 판정 (설정된 시간동안 유지)
        float timer = 0f;
        bool hasHit = false;

        PlayEffect(peckHitEffect, transform.position + attackDir * attackRange, Quaternion.identity);

        while (timer < peckActiveTime)
        {
            if (!hasHit)
            {
                if (CheckDamage(transform.position, attackRange, peckAngle / 2f, peckDamage, attackDir))
                    hasHit = true; 
            }
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 6. 후딜레이 (남은 시간 계산해서 대기)
        float usedTime = peckWindupTime + peckActiveTime;
        float remainingTime = peckTotalTime - usedTime;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);
    }

    // --- 패턴 2: 돌진 (총 2.08초) ---
    IEnumerator Pattern_Dash()
    {
        Vector3 dashDir = GetDirToPlayer(); 
        
        GameObject indicator = null;
        if (dashIndicator != null)
        {
            // 돌진 거리는 속도 * 시간
            float totalDashDistance = dashSpeed * dashMoveTime;

            Vector3 spawnPos = transform.position + dashDir * (totalDashDistance / 2);
            Quaternion indicatorRot = Quaternion.LookRotation(dashDir);
            indicator = Instantiate(dashIndicator, spawnPos, indicatorRot);
            
            Vector3 pos = indicator.transform.position; pos.y = 0.1f; indicator.transform.position = pos;

            float originalY = indicator.transform.localScale.y;
            indicator.transform.localScale = new Vector3(dashWidth, originalY, totalDashDistance);
        }

        // 1. 준비 시간 대기
        yield return new WaitForSeconds(dashWindupTime);

        // 2. 출발 직전 재조준
        LookAtPlayerInstant();
        dashDir = GetDirToPlayer();

        if (indicator != null) Destroy(indicator);

        PlaySound(dashStartSound);
        anim.SetTrigger("AttackDash");
        
        GameObject trail = null;
        if (dashTrailEffect != null)
            trail = Instantiate(dashTrailEffect, transform.position, transform.rotation, transform);

        // 3. 돌진 이동
        float currentDashTime = 0f;

        while (currentDashTime < dashMoveTime)
        {
            rb.MovePosition(rb.position + dashDir * dashSpeed * Time.fixedDeltaTime);
            
            if (CheckDamage(transform.position, dashWidth, 360f, dashDamage, dashDir))
            {
                PlaySound(dashHitSound);
                PlayEffect(dashHitEffect, transform.position + dashDir, Quaternion.identity);
                break; 
            }

            currentDashTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (trail != null) Destroy(trail);
        rb.linearVelocity = Vector3.zero;

        // 4. 후딜레이
        float usedTime = dashWindupTime + dashMoveTime;
        float remainingTime = dashTotalTime - usedTime;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);
    }

    // --- 패턴 3: 점프 (총 2.08초) ---
    IEnumerator Pattern_JumpSlam()
    {
        anim.SetTrigger("AttackJump");
        PlaySound(jumpUpSound);
        PlayEffect(jumpStartEffect, transform.position, Quaternion.identity);

        // 1. 도약 준비 대기
        yield return new WaitForSeconds(jumpWindupTime);

        // 2. 공중 체공 (착지 위치 계산)
        Vector3 landingPos = player.position; 
        
        GameObject indicator = null;
        if (jumpIndicator != null)
        {
            indicator = Instantiate(jumpIndicator, landingPos, Quaternion.identity);
            Vector3 pos = indicator.transform.position; pos.y = 0.1f; indicator.transform.position = pos;

            float diameter = jumpRadius * 2;
            float originalY = indicator.transform.localScale.y;
            indicator.transform.localScale = new Vector3(diameter, originalY, diameter);
        }

        // 체공 시간 대기
        yield return new WaitForSeconds(jumpAirTime);

        // 3. 착지 (쾅!)
        if (indicator != null) Destroy(indicator);
        transform.position = landingPos; 
        
        PlaySound(jumpLandSound);
        PlayEffect(jumpLandEffect, transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(transform.position, jumpRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player")) GiveDamage(hit.transform, jumpDamage);
        }

        // 4. 후딜레이 (착지 모션)
        float usedTime = jumpWindupTime + jumpAirTime;
        float remainingTime = jumpTotalTime - usedTime;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);
    }

    void PlaySound(AudioClip clip) { if (clip != null && audioSource != null) audioSource.PlayOneShot(clip); }
    void PlayEffect(GameObject prefab, Vector3 pos, Quaternion rot) { if (prefab != null) Instantiate(prefab, pos, rot); }

    bool CheckDamage(Vector3 center, float range, float angle, float damage, Vector3 forwardDir)
    {
        float dist = Vector3.Distance(center, player.position);
        if (dist <= range)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            if (Vector3.Angle(forwardDir, dirToPlayer) < angle)
            {
                GiveDamage(player, damage);
                return true; 
            }
        }
        return false; 
    }

    void GiveDamage(Transform target, float amount)
    {
        HealthBar hp = target.GetComponent<HealthBar>();
        if (hp != null) hp.TakeDamage(amount);
    }

    void OnDrawGizmos()
    {
        // 1. 점프 범위
        Gizmos.color = new Color(0, 0, 1, 0.2f);
        Gizmos.DrawSphere(transform.position, jumpRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, jumpRadius);

        // 2. 쪼기 범위
        Vector3 forward = transform.forward;
        if (Application.isPlaying && player != null)
        {
             Vector3 dir = GetDirToPlayer();
             if(dir != Vector3.zero) forward = dir;
        }
        else 
        {
            forward = Quaternion.Euler(0, modelRotationOffset, 0) * transform.forward;
        }

        Gizmos.color = Color.yellow;
        Vector3 leftDir = Quaternion.Euler(0, -peckAngle / 2, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, peckAngle / 2, 0) * forward;

        Gizmos.DrawRay(transform.position, leftDir * attackRange);
        Gizmos.DrawRay(transform.position, rightDir * attackRange);
        Gizmos.DrawLine(transform.position + leftDir * attackRange, transform.position + rightDir * attackRange);

        // 3. 돌진 폭
        Gizmos.color = Color.green;
        Vector3 dashEndPos = transform.position + forward * 5.0f; 
        Vector3 rightOffset = Vector3.Cross(Vector3.up, forward).normalized * (dashWidth); 
        Gizmos.DrawLine(transform.position - rightOffset, dashEndPos - rightOffset);
        Gizmos.DrawLine(transform.position + rightOffset, dashEndPos + rightOffset);
    }
}
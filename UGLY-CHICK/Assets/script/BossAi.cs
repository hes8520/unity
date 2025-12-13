using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class BossAI : MonoBehaviour
{
    [Header("--- ★ 모델 스왑 & 애니메이터 설정 ---")]
    public GameObject riggedModel;   // 평소/뼈대 있는 모델 그룹 (RiggedModel_Group)
    public GameObject noBoneModel;   // 변신/뼈대 없는 모델 (Boss_Chicken_2skill)
    
    [Header("--- ★ 변신 모델 애니메이션 설정 ---")]
    public Animator noBoneAnim;      // ★ Boss_Chicken_2skill에 붙은 Animator 컴포넌트 연결
    public string dashStateName = "Dash"; // 변신 모델의 돌진 애니메이션 State 이름 (대소문자 정확히)
    public string jumpStateName = "Jump"; // 변신 모델의 점프 애니메이션 State 이름 (대소문자 정확히)

    [Header("--- ★ 시간 설정 (초 단위) ---")]
    [Header("[1. 쪼기 시간]")]
    public float peckTotalTime = 4.1f;
    public float peckWindupTime = 1.88f;
    public float peckActiveTime = 1.23f;

    [Header("[2. 돌진 시간]")]
    public float dashTotalTime = 2.08f;
    public float dashWindupTime = 0.28f;
    public float dashMoveTime = 1.44f;

    [Header("[3. 점프 시간]")]
    public float jumpTotalTime = 2.08f;
    public float jumpWindupTime = 1.07f;
    public float jumpAirTime = 0.4f;

    [Header("--- 기타 설정 ---")]
    public float modelRotationOffset = 0f;
    [Range(0, 360)] public float peckAngle = 40f;
    public float dashWidth = 2.0f;
    public float jumpRadius = 5.0f;

    public Transform player;
    public float moveSpeed = 3.5f;
    public float rotSpeed = 10f;
    public float detectionRange = 15f;
    public float attackRange = 3.0f;
    public float attackCooldown = 3.0f;

    public float peckDamage = 10f;
    public float dashDamage = 20f;
    public float jumpDamage = 30f;
    public float dashSpeed = 15f;

    [Header("사운드 & 이펙트")]
    public AudioClip peckSound;
    public AudioClip dashStartSound, dashHitSound, jumpUpSound, jumpLandSound;
    public GameObject peckHitEffect, dashTrailEffect, dashHitEffect, jumpStartEffect, jumpLandEffect;
    public GameObject peckIndicator, dashIndicator, jumpIndicator;

    private Animator mainAnim; // 본체(뼈대) 애니메이터
    private Rigidbody rb;
    private AudioSource audioSource;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;

    void Start()
    {
        // 최상위(본체) 애니메이터 가져오기 (이건 걷기/쪼기용)
        mainAnim = GetComponentInChildren<Animator>(); 
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // 시작 시 기본 모델(뼈대 있음)만 켜기
        SwitchModel(true);
    }

    // ★ 모델 교체 함수
    void SwitchModel(bool useRigged)
    {
        if (riggedModel != null) riggedModel.SetActive(useRigged);
        if (noBoneModel != null) noBoneModel.SetActive(!useRigged);
    }

    void FixedUpdate()
    {
        if (isAttacking || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        Vector3 dirToPlayer = GetDirToPlayer();

        // 회전 처리
        if (dirToPlayer != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dirToPlayer);
            Quaternion targetRot = lookRot * Quaternion.Euler(0, modelRotationOffset, 0);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotSpeed * Time.fixedDeltaTime));
        }

        // 이동 및 공격 판단
        if (dist <= attackRange)
        {
            if (mainAnim != null) mainAnim.SetBool("IsMove", false);
            
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(ChooseAttackPattern());
            }
        }
        else if (dist <= detectionRange)
        {
            if (mainAnim != null) mainAnim.SetBool("IsMove", true);
            
            Vector3 movePos = transform.position + dirToPlayer * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(movePos);
        }
        else
        {
            if (mainAnim != null) mainAnim.SetBool("IsMove", false);
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
        // 유니티 버전에 따라 velocity 또는 linearVelocity 사용
        rb.linearVelocity = Vector3.zero; 

        int pattern = Random.Range(0, 3);

        switch (pattern)
        {
            case 0: yield return StartCoroutine(Pattern_Peck()); break;
            case 1: yield return StartCoroutine(Pattern_Dash()); break;
            case 2: yield return StartCoroutine(Pattern_JumpSlam()); break;
        }

        yield return new WaitForSeconds(0.5f);
        
        // ★ 패턴 종료 후 안전하게 본체 모델로 복귀
        SwitchModel(true); 
        isAttacking = false;
    }

    // --- 패턴 1: 쪼기 (본체 사용) ---
    IEnumerator Pattern_Peck()
    {
        SwitchModel(true); // 본체 켜기
        
        if (mainAnim != null) mainAnim.SetTrigger("AttackNormal");
        PlaySound(peckSound);

        Vector3 attackDir = GetDirToPlayer();
        GameObject indicator = null;
        if (peckIndicator != null)
        {
            Quaternion indicatorRot = Quaternion.LookRotation(attackDir);
            Vector3 spawnPos = transform.position + attackDir * (attackRange / 2);
            indicator = Instantiate(peckIndicator, spawnPos, indicatorRot);
            Vector3 pos = indicator.transform.position; pos.y = 0.1f; indicator.transform.position = pos;
            indicator.transform.localScale = new Vector3(attackRange, indicator.transform.localScale.y, attackRange);
        }

        yield return new WaitForSeconds(peckWindupTime);

        LookAtPlayerInstant();
        attackDir = GetDirToPlayer();
        if (indicator != null) Destroy(indicator);

        float timer = 0f;
        bool hasHit = false;
        PlayEffect(peckHitEffect, transform.position + attackDir * attackRange, Quaternion.identity);

        while (timer < peckActiveTime)
        {
            if (!hasHit)
            {
                if (CheckDamage(transform.position, attackRange, peckAngle / 2f, peckDamage, attackDir)) hasHit = true;
            }
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        float remaining = peckTotalTime - (peckWindupTime + peckActiveTime);
        if (remaining > 0) yield return new WaitForSeconds(remaining);
    }

    // --- 패턴 2: 돌진 (★ 변신 모델 사용) ---
    IEnumerator Pattern_Dash()
    {
        SwitchModel(false); // 변신 모델 켜기

        // ★ 변신 모델의 애니메이터를 직접 실행
        if (noBoneAnim != null) 
        {
            noBoneAnim.Play(dashStateName, 0, 0f); 
        }

        Vector3 dashDir = GetDirToPlayer();
        GameObject indicator = null;
        if (dashIndicator != null)
        {
            float totalDashDistance = dashSpeed * dashMoveTime;
            Vector3 spawnPos = transform.position + dashDir * (totalDashDistance / 2);
            Quaternion indicatorRot = Quaternion.LookRotation(dashDir);
            indicator = Instantiate(dashIndicator, spawnPos, indicatorRot);
            Vector3 pos = indicator.transform.position; pos.y = 0.1f; indicator.transform.position = pos;
            indicator.transform.localScale = new Vector3(dashWidth, indicator.transform.localScale.y, totalDashDistance);
        }

        yield return new WaitForSeconds(dashWindupTime);

        LookAtPlayerInstant();
        dashDir = GetDirToPlayer();
        if (indicator != null) Destroy(indicator);

        PlaySound(dashStartSound);

        GameObject trail = null;
        if (dashTrailEffect != null)
            trail = Instantiate(dashTrailEffect, transform.position, transform.rotation, transform);

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

        float remaining = dashTotalTime - (dashWindupTime + dashMoveTime);
        if (remaining > 0) yield return new WaitForSeconds(remaining);
        
        SwitchModel(true); // 복귀
    }

    // --- 패턴 3: 점프 (★ 변신 모델 사용) ---
    IEnumerator Pattern_JumpSlam()
    {
        SwitchModel(false); // 변신 모델 켜기

        // ★ 변신 모델 애니메이터 직접 실행
        if (noBoneAnim != null)
        {
            noBoneAnim.Play(jumpStateName, 0, 0f);
        }

        PlaySound(jumpUpSound);
        PlayEffect(jumpStartEffect, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(jumpWindupTime);

        Vector3 landingPos = player.position;
        GameObject indicator = null;
        if (jumpIndicator != null)
        {
            indicator = Instantiate(jumpIndicator, landingPos, Quaternion.identity);
            Vector3 pos = indicator.transform.position; pos.y = 0.1f; indicator.transform.position = pos;
            float diameter = jumpRadius * 2;
            indicator.transform.localScale = new Vector3(diameter, indicator.transform.localScale.y, diameter);
        }

        yield return new WaitForSeconds(jumpAirTime);

        if (indicator != null) Destroy(indicator);
        transform.position = landingPos;
        PlaySound(jumpLandSound);
        PlayEffect(jumpLandEffect, transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(transform.position, jumpRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player")) GiveDamage(hit.transform, jumpDamage);
        }

        float remaining = jumpTotalTime - (jumpWindupTime + jumpAirTime);
        if (remaining > 0) yield return new WaitForSeconds(remaining);
        
        SwitchModel(true); // 복귀
    }

    // --- 유틸리티 함수들 ---
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
        // 체력바 스크립트가 있다면 데미지 전달
        // (HealthBar 스크립트 이름을 사용하시는 것에 맞춰 변경하세요)
        var hp = target.GetComponent<HealthBar>();
        if (hp != null) hp.TakeDamage(amount);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.2f);
        Gizmos.DrawSphere(transform.position, jumpRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, jumpRadius);

        Vector3 forward = transform.forward;
        if (Application.isPlaying && player != null)
        {
            Vector3 dir = GetDirToPlayer();
            if (dir != Vector3.zero) forward = dir;
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

        Gizmos.color = Color.green;
        Vector3 dashEndPos = transform.position + forward * 5.0f;
        Vector3 rightOffset = Vector3.Cross(Vector3.up, forward).normalized * (dashWidth);
        Gizmos.DrawLine(transform.position - rightOffset, dashEndPos - rightOffset);
        Gizmos.DrawLine(transform.position + rightOffset, dashEndPos + rightOffset);
    }
}
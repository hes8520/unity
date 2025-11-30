using UnityEngine;
using UnityEngine.UI; 

public class LockOnSystem : MonoBehaviour
{
    [Header("설정")]
    public float searchRadius = 20f;    
    public LayerMask enemyLayer;        
    public Transform currentTarget;     
    public bool isLockedOn = false;     

    [Header("UI (선택사항)")]
    public Image lockOnIcon;            

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        if (lockOnIcon != null) lockOnIcon.gameObject.SetActive(false);
    }

    void Update()
    {
        // 락온 토글 (Tab 키)
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetMouseButtonDown(2))
        {
            if (isLockedOn)
                Unlock();
            else
                FindTarget();
        }

        // 아이콘 표시 로직
        if (isLockedOn && currentTarget != null)
        {
            if (lockOnIcon != null)
            {
                lockOnIcon.gameObject.SetActive(true);
                Vector3 screenPos = mainCam.WorldToScreenPoint(currentTarget.position + Vector3.up * 2f);
                lockOnIcon.transform.position = screenPos;
            }

            if (!currentTarget.gameObject.activeInHierarchy)
            {
                Unlock();
            }
        }
        else
        {
            if (lockOnIcon != null) lockOnIcon.gameObject.SetActive(false);
        }
    }

    void FindTarget()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, searchRadius, enemyLayer);
        
        float closestDist = Mathf.Infinity;
        Transform bestTarget = null;

        foreach (Collider enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                bestTarget = enemy.transform;
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            isLockedOn = true;
            Debug.Log("락온 대상: " + currentTarget.name);

            // ★ [추가] 락온된 적의 BossHealth를 찾아서 체력바를 켭니다.
            BossHealth bossHealth = currentTarget.GetComponent<BossHealth>();
            if (bossHealth != null)
            {
                bossHealth.ShowHealthBar(true); // 켜기!
            }
        }
    }

    public void Unlock()
    {
        // ★ [추가] 락온을 풀기 전에, 기존 타겟의 체력바를 끕니다.
        if (currentTarget != null)
        {
            BossHealth bossHealth = currentTarget.GetComponent<BossHealth>();
            if (bossHealth != null)
            {
                bossHealth.ShowHealthBar(false); // 끄기!
            }
        }

        isLockedOn = false;
        currentTarget = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}
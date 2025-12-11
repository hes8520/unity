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
        // 락온 토글 (Tab 키 또는 휠 버튼)
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
                // 타겟 머리 위쪽으로 아이콘 위치 갱신
                Vector3 screenPos = mainCam.WorldToScreenPoint(currentTarget.position + Vector3.up * 2f);
                lockOnIcon.transform.position = screenPos;
            }

            // 타겟이 죽거나 사라지면 락온 해제
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

        // 가장 가까운 적 찾기
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

            // [삭제됨] 여기서 체력바를 켜던 코드를 없앴습니다.
        }
    }

    public void Unlock()
    {
        // [삭제됨] 여기서 체력바를 끄던 코드를 없앴습니다.

        isLockedOn = false;
        currentTarget = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;       

    [Header("Lock On")]
    public LockOnSystem lockOnSystem; 

    [Header("Camera Position Settings")]
    public Vector3 normalOffset = new Vector3(0, 2f, -4f);   
    public Vector3 lockOnOffset = new Vector3(0.8f, 1.8f, -2.5f); // 락온 시 약간 더 가깝게
    public float offsetChangeSpeed = 5f; 
    
    private Vector3 currentOffset; 

    [Header("Settings")]
    public float sensitivity = 5f; 
    public float smoothSpeed = 10f; 

    float pitch; 
    float yaw;   

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pitch = transform.eulerAngles.x;
        yaw = transform.eulerAngles.y;

        if (lockOnSystem == null && target != null)
            lockOnSystem = target.GetComponent<LockOnSystem>();
            
        currentOffset = normalOffset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Quaternion targetRotation;
        Vector3 targetOffsetVal; 

        // 1. 락온 상태 확인
        if (lockOnSystem != null && lockOnSystem.isLockedOn && lockOnSystem.currentTarget != null)
        {
            // [락온 모드]
            Vector3 lookTargetPos;

            // ★ [핵심 변경] 적의 Collider(몸체)를 찾아서 그 정중앙(Center)을 가져옵니다.
            Collider enemyCollider = lockOnSystem.currentTarget.GetComponent<Collider>();
            
            if (enemyCollider != null)
            {
                // 콜라이더가 있다면 그 박스의 정중앙을 목표로 함
                lookTargetPos = enemyCollider.bounds.center;
            }
            else
            {
                // 콜라이더가 없다면 그냥 발바닥 + 1.5m
                lookTargetPos = lockOnSystem.currentTarget.position + Vector3.up * 1.5f;
            }

            // 카메라 위치에서 -> 적의 정중앙을 향하는 방향 계산
            Vector3 dirToTarget = lookTargetPos - transform.position;
            
            Quaternion lookRotation = Quaternion.LookRotation(dirToTarget);
            targetRotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            
            pitch = targetRotation.eulerAngles.x;
            yaw = targetRotation.eulerAngles.y;

            targetOffsetVal = lockOnOffset;
        }
        else
        {
            // [일반 모드]
            yaw += Input.GetAxis("Mouse X") * sensitivity;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity;
            pitch = Mathf.Clamp(pitch, -30f, 60f);

            targetRotation = Quaternion.Euler(pitch, yaw, 0);

            targetOffsetVal = normalOffset;
        }

        // 2. 위치 및 오프셋 적용 (부드럽게)
        currentOffset = Vector3.Lerp(currentOffset, targetOffsetVal, Time.deltaTime * offsetChangeSpeed);
        
        Vector3 desiredPosition = target.position + targetRotation * currentOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 3. 회전 적용
        transform.rotation = targetRotation;
    }
}
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;       // 따라다닐 플레이어
    public Vector3 offset = new Vector3(0, 2f, -5f); // 플레이어와의 거리 및 높이

    [Header("Settings")]
    public float sensitivity = 5f; // 마우스 감도
    public float smoothSpeed = 10f; // 카메라 따라가는 속도

    // 마우스 회전값 누적 변수
    float pitch; // 상하 회전 (Y축)
    float yaw;   // 좌우 회전 (X축)

    void Start()
    {
        // 시작 시 현재 카메라 각도에 맞춰 초기화
        pitch = transform.eulerAngles.x;
        yaw = transform.eulerAngles.y;

        // (선택사항) 마우스 커서를 화면에 가두고 숨기기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        // 2. 회전값 계산
        yaw += mouseX;
        pitch -= mouseY; // 마우스를 올리면 위를 봐야 하므로 뺌 (Invert 안함)
        
        // 상하 회전 제한 (너무 위나 아래로 꺾이지 않게 -30도 ~ 60도)
        pitch = Mathf.Clamp(pitch, -30f, 60f);

        // 3. 회전 적용
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // 4. 위치 계산 (플레이어 위치 + 회전된 오프셋)
        // LateUpdate를 써야 플레이어가 움직인 뒤에 카메라가 따라가서 덜덜거림이 없음
        Vector3 desiredPosition = target.position + rotation * offset;
        
        // 5. 부드러운 이동 (Lerp) 및 위치 적용
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // 6. 항상 플레이어를 바라보게 함 (또는 회전값 그대로 사용)
        transform.LookAt(target.position + Vector3.up * 1.5f); // 플레이어의 가슴 쯤을 바라보게 보정
    }
}
using UnityEngine;

public class TextWobble : MonoBehaviour
{
    [Header("최대 흔들림 각도 (Z축)")]
    public float maxRotationAngle = 6.0f; // 사용자가 요청한 6도

    [Header("흔들림 속도")]
    public float rotationSpeed = 5.0f; // 이 값을 조절해 속도를 변경

    // 이 스크립트의 로직은 오브젝트의 '로컬' 회전값을 기준으로 합니다.
    // 만약 부모 오브젝트가 회전하면 그에 맞춰 함께 흔들립니다.
    
    private Quaternion initialLocalRotation; // 시작 시의 로컬 회전값

    void Start()
    {
        // 스크립트가 시작될 때의 현재 로컬 회전값을 저장합니다.
        // (transform.rotation 대신 localRotation을 사용해야
        // 부모 오브젝트의 회전에 영향을 받지 않습니다.)
        initialLocalRotation = transform.localRotation;
    }

    void Update()
    {
        // 1. Sin 함수를 이용해 -1.0 ~ 1.0 사이를 부드럽게 왕복하는 값 생성
        // Time.time * rotationSpeed : 시간에 따라 속도를 곱해 Sin 그래프를 탐색
        float wobble = Mathf.Sin(Time.time * rotationSpeed);

        // 2. 왕복하는 값(-1 ~ 1)을 최대 각도(-6 ~ 6)로 변환
        float currentZRotation = wobble * maxRotationAngle;

        // 3. 시작 시의 로컬 회전값(initialLocalRotation)에
        //    Z축 흔들림(currentZRotation)을 '더해(곱해)' 줍니다.
        //    Quaternion.Euler(0, 0, currentZRotation) : Z축으로만 회전하는 쿼터니언 생성
        transform.localRotation = initialLocalRotation * Quaternion.Euler(0, 0, currentZRotation);
    }
}
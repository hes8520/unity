using UnityEngine;
using TMPro; 

public class TextShake2D : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("얼마나 세게 흔들 건지 (0.1 ~ 10 추천)")]
    public float shakePower = 5f; 

    [Tooltip("얼마나 빨리 흔들 건지")]
    public float shakeSpeed = 20f;

    private Vector3 originalPos;
    private float randomSeed;

    void Start()
    {
        // 시작할 때 원래 위치를 기억해둡니다.
        originalPos = transform.localPosition;
        randomSeed = Random.Range(0f, 100f);
    }

    void Update()
    {
        // 시간 흐름에 따라 불규칙한 값을 만듭니다.
        float timer = Time.time * shakeSpeed;

        // X축 흔들림 (좌우)
        float xShake = (Mathf.PerlinNoise(timer, randomSeed) - 0.5f) * 2 * shakePower;
        
        // Y축 흔들림 (상하) - 시드값을 다르게 줘서 X랑 따로 놀게 함
        float yShake = (Mathf.PerlinNoise(randomSeed, timer) - 0.5f) * 2 * shakePower;

        // 원래 위치 + 흔들림 값 적용 (Z축은 건드리지 않음)
        transform.localPosition = new Vector3(originalPos.x + xShake, originalPos.y + yShake, originalPos.z);
    }
}
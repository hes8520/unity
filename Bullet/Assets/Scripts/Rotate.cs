using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotateSpeed = 30f; // 초당 회전 속도 (도 단위)

    void Update()
    {
        // Y축 기준으로 회전 (원형 회전)
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
}

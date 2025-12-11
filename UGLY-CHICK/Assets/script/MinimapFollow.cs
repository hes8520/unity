using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player; // 플레이어 위치

    // 플레이어와 같이 회전할지 여부 (true면 네비게이션처럼 돔, false면 북쪽 고정)
    public bool rotateWithPlayer = false; 

    void LateUpdate()
    {
        if (player == null) return;

        // 1. 위치 따라가기 (높이 Y는 유지!)
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y; // 카메라의 원래 높이 유지
        transform.position = newPosition;

        // 2. 회전 따라가기 (선택 사항)
        if (rotateWithPlayer)
        {
            // Y축(좌우) 회전만 따라감
            Vector3 newRotation = transform.eulerAngles;
            newRotation.y = player.eulerAngles.y;
            transform.rotation = Quaternion.Euler(90f, newRotation.y, 0f);
        }
    }
}
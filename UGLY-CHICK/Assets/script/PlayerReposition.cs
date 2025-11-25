using UnityEngine;
using System.Collections;

public class PlayerReposition : MonoBehaviour
{
    IEnumerator Start()
    {
        if (GameManager.Instance == null) yield break;

        if (GameManager.Instance.isReturning)
        {
            yield return new WaitForSeconds(0.1f); // 0.1초 대기

            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // ★ 위치와 회전 동시에 적용
            transform.position = GameManager.Instance.savedPosition;
            transform.rotation = GameManager.Instance.savedRotation; // ★ 추가된 부분

            Physics.SyncTransforms(); // 물리 강제 동기화

            if (cc != null) cc.enabled = true;

            GameManager.Instance.isReturning = false;
        }
    }
}
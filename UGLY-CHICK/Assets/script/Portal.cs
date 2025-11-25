using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("설정")]
    public string bossSceneName;
    public Transform returnPoint; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                // 1. 위치와 회전 모두 저장
                GameManager.Instance.savedPosition = returnPoint.position;
                GameManager.Instance.savedRotation = returnPoint.rotation; // ★ 추가된 부분
                
                GameManager.Instance.sceneToReturn = SceneManager.GetActiveScene().name;
                GameManager.Instance.isReturning = true;
            }

            SceneManager.LoadScene(bossSceneName);
        }
    }
}
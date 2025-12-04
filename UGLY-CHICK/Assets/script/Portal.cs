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
            // 데이터 저장 부분 (기존과 동일)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.savedPosition = returnPoint.position;
                GameManager.Instance.savedRotation = returnPoint.rotation;
                
                GameManager.Instance.sceneToReturn = SceneManager.GetActiveScene().name;
                GameManager.Instance.isReturning = true;
            }

            // ▼▼▼ [수정된 부분] ▼▼▼
            // 페이드 효과가 있으면 페이드 아웃 후 이동, 없으면 그냥 이동
            SceneFader fader = FindAnyObjectByType<SceneFader>();

            if (fader != null)
            {
                fader.ChangeScene(bossSceneName); // 부드럽게 이동
            }
            else
            {
                SceneManager.LoadScene(bossSceneName); // 비상용 (그냥 이동)
            }
            // ▲▲▲ [수정 끝] ▲▲▲
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 로드를 위해 필수
using System.Collections; // Coroutine을 사용할 경우 포함

public class GameOverManager : MonoBehaviour
{
    // Inspector에서 게임 오버 UI 패널을 드래그하여 연결할 변수
    public GameObject gameOverPanel;

    private string currentSceneName;

    void Awake()
    {
        // 현재 로드된 씬 이름을 저장합니다. (재시작 시 사용)
        currentSceneName = SceneManager.GetActiveScene().name;

        // 게임 시작 시 UI는 비활성화합니다.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // TimeScale이 0으로 설정되어 있을 수 있으므로 시작 시 1로 설정합니다.
        Time.timeScale = 1f;
    }

    //  1. 게임 오버 화면을 띄우는 함수 (다른 스크립트에서 호출됨)
    public void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 게임 일시 정지 (선택 사항: 게임 플레이를 멈춥니다)
        Time.timeScale = 0f;

        // 마우스 커서 활성화 (UI 조작을 위해 필요)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //  2. 재시작 버튼에 연결될 함수
    public void RestartGame()
    {
        // 시간을 다시 원래대로 되돌립니다. (Time.timeScale = 0f를 해제)
        Time.timeScale = 1f;

        // 현재 씬을 다시 로드합니다.
        SceneManager.LoadScene(currentSceneName);
    }

    //  3. 게임 종료 버튼에 연결될 함수
    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다.");

        // Unity 에디터에서 실행 중일 경우 플레이 모드를 종료합니다.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        // 빌드된 게임일 경우 애플리케이션을 종료합니다.
#else
            Application.Quit();
#endif
    }
}
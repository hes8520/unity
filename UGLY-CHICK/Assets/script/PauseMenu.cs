using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI 패널 연결")]
    public GameObject pauseMenuPanel; // 버튼 4개 있는 패널
    public GameObject settingsPanel;  // 설정 창 패널

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 1. 설정 창이 켜져 있다면 -> 설정 끄고 메뉴로 돌아오기
            if (settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            // 2. 이미 일시정지 상태라면 -> 게임 재개
            else if (isPaused)
            {
                ResumeGame();
            }
            // 3. 게임 중이라면 -> 일시정지
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false); // 혹시 켜져있다면 같이 끔
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.visible = false; 
        Cursor.lockState = CursorLockMode.Locked;
    }

    void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.visible = true; 
        Cursor.lockState = CursorLockMode.None;
    }

    // --- 버튼 기능들 ---

    // [SETTINGS] 버튼에 연결: 설정 창 열기
    public void OpenSettings()
    {
        pauseMenuPanel.SetActive(false); // 기존 메뉴 숨김
        settingsPanel.SetActive(true);   // 설정 창 보임
    }

    // [설정 창의 Back] 버튼에 연결: 설정 창 닫기
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);  // 설정 창 숨김
        pauseMenuPanel.SetActive(true);  // 기존 메뉴 다시 보임
    }

    // [MAIN MENU] 버튼에 연결
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("main"); 

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // [QUIT] 버튼에 연결
    public void QuitGame()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }
}
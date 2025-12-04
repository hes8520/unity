using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 1.5f;

    private bool isFadingIn = false;
    private bool isFadingOut = false;
    private string sceneToMove = "";

    // ★ 중요: 씬이 로드될 때마다 이벤트를 연결함
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // ★ 중요: 오브젝트가 꺼지거나 파괴될 때 이벤트 연결 해제 (에러 방지)
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ★★★ 핵심: 씬 로딩이 끝나면 자동으로 실행되는 함수 ★★★
    // GameManager에 붙어있어도, 씬이 바뀔 때마다 이 함수는 무조건 실행됩니다.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0, 0, 0, 1); // 화면을 검은색으로 강제 설정
            isFadingIn = true;  // "밝아져라!" 명령 시작
            isFadingOut = false;
        }
    }

    void Update()
    {
        // 1. 페이드 인 (검 -> 투명)
        if (isFadingIn)
        {
            if (fadeImage.color.a > 0)
            {
                // 시간 멈춤(Time.timeScale=0) 무시하고 작동하도록 unscaledDeltaTime 사용
                float newAlpha = fadeImage.color.a - (Time.unscaledDeltaTime * fadeSpeed);
                fadeImage.color = new Color(0, 0, 0, newAlpha);
            }
            else
            {
                // 다 투명해지면 끝
                fadeImage.color = new Color(0, 0, 0, 0);
                fadeImage.gameObject.SetActive(false);
                isFadingIn = false; 
            }
        }

        // 2. 페이드 아웃 (투명 -> 검)
        if (isFadingOut)
        {
            if (!fadeImage.gameObject.activeSelf) fadeImage.gameObject.SetActive(true);

            if (fadeImage.color.a < 1)
            {
                float newAlpha = fadeImage.color.a + (Time.unscaledDeltaTime * fadeSpeed);
                fadeImage.color = new Color(0, 0, 0, newAlpha);
            }
            else
            {
                isFadingOut = false;
                SceneManager.LoadScene(sceneToMove);
            }
        }
    }

    public void ChangeScene(string sceneName)
    {
        sceneToMove = sceneName;
        isFadingOut = true;
        isFadingIn = false;
    }
}
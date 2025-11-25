using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // ★ 글자 기능을 쓰려면 이게 필수입니다!

public class BossCutscene : MonoBehaviour
{
    [Header("연결할 것들")]
    public Animator bossAnimator;      // 보스 애니메이터
    public GameObject dialoguePanel;   // 대화창 배경 (껐다 켰다 할 거임)
    public TextMeshProUGUI dialogueText; // 실제 글자

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // 0. 시작할 땐 대화창 숨기기
        dialoguePanel.SetActive(false);

        // 1. 보스 등장 애니메이션 (2초 대기)
        if(bossAnimator != null) bossAnimator.SetTrigger("Intro");
        yield return new WaitForSeconds(2.0f);

        // 2. 대화창 켜기
        dialoguePanel.SetActive(true);

        // 3. 첫 번째 대사 출력
        dialogueText.text = "침입자가 감히 내 영역에...";
        yield return new WaitForSeconds(3.0f); // 3초 동안 보여줌

        // 4. 두 번째 대사 출력
        dialogueText.text = "살아서 돌아갈 생각은 마라!!";
        yield return new WaitForSeconds(3.0f);

        // 5. 대화창 끄기
        dialoguePanel.SetActive(false);

        // 4. 아까 저장해둔 원래 씬으로 복귀
        string originalScene = GameManager.Instance.sceneToReturn;
        SceneManager.LoadScene(originalScene);
    }
}
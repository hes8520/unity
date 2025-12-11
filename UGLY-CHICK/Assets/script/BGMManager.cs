using UnityEngine;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [Header("음악 설정")]
    public AudioSource audioSource; // 음악을 재생할 스피커
    public AudioClip normalBGM;     // 평상시 브금
    public AudioClip battleBGM;     // 전투 브금
    public float fadeDuration = 1.0f; // 음악 바뀌는 속도 (초)

    [Header("상태 확인 (수정 X)")]
    public int enemyCount = 0; // 나를 노리는 몬스터 수
    private bool isBattleState = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        // 처음에 평상시 음악 재생
        if (audioSource != null && normalBGM != null)
        {
            audioSource.clip = normalBGM;
            audioSource.Play();
            audioSource.loop = true;
        }
    }

    // 몬스터가 "나 너 봤어!" 하고 신고하는 함수
    public void AddEnemyAggro()
    {
        enemyCount++;
        CheckState();
    }

    // 몬스터가 죽거나 멀어져서 신고 취소하는 함수
    public void RemoveEnemyAggro()
    {
        enemyCount--;
        if (enemyCount < 0) enemyCount = 0; // 안전장치
        CheckState();
    }

    // 상태 점검: 전투냐 평화냐?
    void CheckState()
    {
        // 적이 1명이라도 있으면 전투 모드
        bool newIsBattle = (enemyCount > 0);

        if (newIsBattle != isBattleState) // 상태가 바뀌었을 때만 실행
        {
            isBattleState = newIsBattle;
            StopAllCoroutines();
            StartCoroutine(CrossFadeMusic(isBattleState ? battleBGM : normalBGM));
        }
    }

    // 음악을 부드럽게 바꾸는 마법 (크로스페이드)
    IEnumerator CrossFadeMusic(AudioClip newClip)
    {
        // 1. 볼륨 줄이기 (Fade Out)
        float startVolume = audioSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }
        audioSource.volume = 0f;

        // 2. 음악 교체
        audioSource.clip = newClip;
        audioSource.Play();

        // 3. 볼륨 키우기 (Fade In)
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }
        audioSource.volume = startVolume;
    }
}
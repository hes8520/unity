using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;

public class BonfireManager : MonoBehaviour
{
    public static BonfireManager Instance;

    [Header("UI 연결")]
    public GameObject bonfireWindow;    // 배경 창
    public Transform buttonList;        // 버튼 정리함
    public GameObject buttonPrefab;     // 버튼 프리팹
    public CanvasGroup uiCanvasGroup;   // 투명도 조절용

    [Header("설정")]
    public float fadeDuration = 0.2f;

    [Header("플레이어")]
    public Transform player;

    private List<Bonfire> discoveredBonfires = new List<Bonfire>();
    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        
        // 시작 시 UI 초기화
        if (bonfireWindow != null) 
        {
            bonfireWindow.SetActive(false);
            if (uiCanvasGroup == null) uiCanvasGroup = bonfireWindow.GetComponent<CanvasGroup>();
            if (uiCanvasGroup != null) uiCanvasGroup.alpha = 0f;
        }
    }

    public void RegisterBonfire(Bonfire bonfire)
    {
        if (!discoveredBonfires.Contains(bonfire)) discoveredBonfires.Add(bonfire);
    }

    public void OpenMenu()
    {
        if (bonfireWindow == null) return;

        // 버튼 초기화 및 생성
        foreach (Transform child in buttonList) Destroy(child.gameObject);
        foreach (Bonfire fire in discoveredBonfires)
        {
            GameObject btn = Instantiate(buttonPrefab, buttonList);
            TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = fire.bonfireName;

            Bonfire targetFire = fire;
            Button btnComp = btn.GetComponent<Button>();
            if (btnComp != null)
            {
                btnComp.onClick.AddListener(() => TeleportTo(targetFire));
            }
        }

        // 페이드 인 시작
        bonfireWindow.SetActive(true);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeUI(true));

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f; // 일시정지
    }

    public void CloseMenu()
    {
        if (bonfireWindow == null) return;
        
        // 페이드 아웃 시작
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeUI(false));

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f; // 게임 재개
    }

    public void TeleportTo(Bonfire targetFire)
    {
        if (targetFire == null || targetFire.spawnPoint == null) return;
        CloseMenu(); // 메뉴 닫기
        StartCoroutine(ForceTeleportRoutine(targetFire.spawnPoint.position)); // 강제 이동
    }

    IEnumerator ForceTeleportRoutine(Vector3 targetPos)
    {
        Time.timeScale = 1f; // 시간 흐름 강제

        NavMeshAgent agent = player.GetComponent<NavMeshAgent>();
        CharacterController cc = player.GetComponent<CharacterController>();
        Rigidbody rb = player.GetComponent<Rigidbody>();
        
        // PlayerMovement 스크립트 끄기 (반항 방지)
        MonoBehaviour moveScript = player.GetComponent("PlayerMovement") as MonoBehaviour; 
        if (moveScript != null) moveScript.enabled = false;

        if (agent != null) agent.enabled = false;
        if (cc != null) cc.enabled = false;
        if (rb != null) { rb.isKinematic = true; rb.linearVelocity = Vector3.zero; }

        player.position = targetPos; // 이동

        yield return null; // 1프레임 대기

        if (agent != null) { agent.enabled = true; agent.Warp(targetPos); agent.ResetPath(); }
        if (cc != null) cc.enabled = true;
        if (rb != null) rb.isKinematic = false;
        if (moveScript != null) moveScript.enabled = true; // 다시 켜기
    }

    IEnumerator FadeUI(bool show)
    {
        float targetAlpha = show ? 1f : 0f;
        float startAlpha = uiCanvasGroup.alpha;
        float time = 0f;
        uiCanvasGroup.blocksRaycasts = show;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime; // 멈춰있어도 시간 흐르게
            uiCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }
        uiCanvasGroup.alpha = targetAlpha;
        if (!show) bonfireWindow.SetActive(false);
    }
}
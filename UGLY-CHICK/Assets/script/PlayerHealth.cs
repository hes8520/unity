using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    // ★ 어디서든(퀵슬롯 등) 내 체력에 접근할 수 있게 만드는 '싱글톤'
    public static HealthBar Instance;

    [Header("UI 연결")]
    public Slider slider;

    [Header("체력 설정")]
    public float maxHealth = 100f;
    private float currentHealth;

    // 데미지 설정
    private const float ENEMY_DAMAGE = 20f;

    // 쿨타임 변수
    private bool canTakeDamage = true;
    public float invincibilityTime = 0.5f;

    // ⚡ 연동을 위한 변수
    private GameOverManager gameOverManager;
    private PlayerMovement playerMovement; // 구르기 무적 체크용

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentHealth = maxHealth;

        // UI 초기화
        if (slider != null)
        {
            slider.maxValue = maxHealth;
            slider.value = currentHealth;
        }

        // ⚡ 필요한 컴포넌트 및 매니저 찾기
        gameOverManager = FindObjectOfType<GameOverManager>();
        playerMovement = GetComponent<PlayerMovement>();

        if (gameOverManager == null)
        {
            Debug.LogError("GameOverManager를 씬에서 찾을 수 없습니다. 게임 오버 기능이 작동하지 않습니다.");
        }
    }

    // 체력 회복 함수 (퀵슬롯 등에서 호출)
    public void Heal(float amount)
    {
        if (currentHealth <= 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        // UI 갱신
        if (slider != null) slider.value = currentHealth;

        Debug.Log($"체력 회복! 현재 체력: {currentHealth}");
    }

    // 데미지를 받는 핵심 함수
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (slider != null) slider.value = currentHealth;

        Debug.Log($"피해를 받았습니다. 잔여 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // ⚡ [수정] 1. 오브젝트가 비활성화 상태이면 코루틴을 시작할 수 없으므로 즉시 종료합니다.
        if (!gameObject.activeInHierarchy) return;

        // 2. 데미지 쿨타임 무적 체크
        if (!canTakeDamage) return;

        // 3. PlayerMovement의 구르기 무적 체크
        if (playerMovement != null && playerMovement.isInvincible)
        {
            return;
        }

        // 4. 충돌 및 데미지 로직 실행
        if (other.gameObject.name == "sword1")
        {
            TakeDamage(ENEMY_DAMAGE);

            // ⚡ [핵심 수정] 5. TakeDamage 호출 후, 플레이어가 아직 살아있는 경우에만 
            // 데미지 쿨타임 코루틴을 시작합니다. (오류 방지)
            if (currentHealth > 0)
            {
                StartCoroutine(DamageCooldown());
            }
        }
    }

    private IEnumerator DamageCooldown()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(invincibilityTime);
        canTakeDamage = true;
    }

    // 플레이어 사망 시 호출되는 함수
    void Die()
    {
        Debug.Log("Player Died! Game Over!");

        // 1. 플레이어 오브젝트의 움직임/콜라이더 등을 비활성화
        gameObject.SetActive(false);

        // 2. 게임 오버 화면을 띄웁니다.
        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOverScreen();
        }
    }
}
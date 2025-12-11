using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    // ★ [추가] 어디서든(퀵슬롯 등) 내 체력에 접근할 수 있게 만드는 '싱글톤'
    public static HealthBar Instance;

    [Header("UI 연결")]
    public Slider slider;

    [Header("체력 설정")]
    public float maxHealth = 100f;
    private float currentHealth;

    private const float ENEMY_DAMAGE = 20f;

    // 쿨타임 변수
    private bool canTakeDamage = true;
    public float invincibilityTime = 0.5f;

    void Awake()
    {
        // ★ [추가] 싱글톤 초기화
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (slider != null)
        {
            slider.maxValue = maxHealth;
            slider.value = currentHealth;
        }
    }

    // ★ [추가] 체력 회복 함수 (퀵슬롯에서 호출)
    public void Heal(float amount)
    {
        if (currentHealth <= 0) return; // 죽었으면 회복 불가

        currentHealth += amount;
        
        // 최대 체력을 넘지 않도록 제한
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        // UI 갱신
        if (slider != null) slider.value = currentHealth;

        Debug.Log($"체력 회복! 현재 체력: {currentHealth}");
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (slider != null) slider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // "sword1"과 접촉 중이며, 데미지를 받을 수 있는 상태인지 확인
        if (other.gameObject.name == "sword1" && canTakeDamage)
        {
            TakeDamage(ENEMY_DAMAGE);
            StartCoroutine(DamageCooldown());
        }
    }

    private IEnumerator DamageCooldown()
    {
        canTakeDamage = false; 
        yield return new WaitForSeconds(invincibilityTime); 
        canTakeDamage = true; 
    }

    void Die()
    {
        Debug.Log("Player Died! Game Over!");
        gameObject.SetActive(false);
    }
}
using UnityEngine;
using UnityEngine.UI; 

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI 연결")]
    public Slider healthSlider; 

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            
            // ★ [추가] 게임 시작할 때 체력바를 일단 숨김
            healthSlider.gameObject.SetActive(false); 
        }
    }

    // ★ [추가] 외부(락온 시스템)에서 체력바를 끄고 켤 수 있게 만드는 함수
    public void ShowHealthBar(bool isVisible)
    {
        if (healthSlider != null)
        {
            healthSlider.gameObject.SetActive(isVisible);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (healthSlider != null)
            healthSlider.gameObject.SetActive(false);

        Destroy(gameObject);
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public Slider healthSlider;
    public float maxHealth = 100f;
    private float currentHealth;
    
    // ★ [추가] 드랍 아이템 설정
    [Header("아이템 드랍")]
    public GameObject dropItemPrefab; // 떨어트릴 아이템 프리팹
    [Range(0, 100)] 
    public int dropChance = 50;       // 드랍 확률 (0~100%)

    private const float BULLET_DAMAGE = 50f;
    private Renderer enemyRenderer;
    private Color originalColor;
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (healthSlider != null) healthSlider.value = currentHealth;

        if (enemyRenderer != null && currentHealth > 0)
            StartCoroutine(FlashColor());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(BULLET_DAMAGE);
            Destroy(other.gameObject);
        }
    }

    private IEnumerator FlashColor()
    {
        enemyRenderer.material.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        enemyRenderer.material.color = originalColor;
    }

    void Die()
    {
        // ★ [수정] 랜덤 확률로 아이템 드랍
        int randomValue = Random.Range(0, 100); // 0부터 99까지 숫자 뽑기

        if (randomValue < dropChance)
        {
            // 당첨!
            if (dropItemPrefab != null)
            {
                // 적 위치보다 살짝 위(Y+1)에 생성
                Instantiate(dropItemPrefab, transform.position + Vector3.up, Quaternion.identity);
                Debug.Log("✨ 아이템 드랍 성공!");
            }
        }
        else
        {
            Debug.Log("💨 꽝! (드랍 실패)");
        }

        Destroy(gameObject);
    }
}
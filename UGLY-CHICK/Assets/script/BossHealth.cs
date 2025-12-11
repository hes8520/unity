using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    public float maxHealth = 1000f;
    public float currentHealth;
    
    [Header("설정")]
    public float battleRange = 20.0f; // 이 거리 안으로 오면 체력바 켜짐

    [Header("UI 연결 (Hierarchy에 있는 슬라이더!)")]
    public Slider healthSlider;      
    public GameObject healthBarUI;   

    private Transform player;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        
        // 1. 슬라이더 설정
        if (healthSlider != null) 
        { 
            healthSlider.maxValue = maxHealth; 
            healthSlider.value = currentHealth; 
        }
        
        // 2. 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // 3. 시작할 때는 일단 숨겨두기 (깜짝 등장 위해)
        if (healthBarUI != null) healthBarUI.SetActive(false);
    }

    void Update()
    {
        if (isDead || player == null || healthBarUI == null) return;

        // 4. 거리 계산
        float distance = Vector3.Distance(transform.position, player.position);
        
        // ★ [핵심] 거리가 가까우면 켜고, 멀면 끈다!
        if (distance <= battleRange) 
        {
            if (!healthBarUI.activeSelf) healthBarUI.SetActive(true);
        }
        else if (distance > battleRange + 10f) 
        {
            // 전투 중이 아닐 때만 끄기 (싸우다 도망가면 꺼짐)
            if (healthBarUI.activeSelf) healthBarUI.SetActive(false);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        // ★ [핵심] 맞으면 거리 상관없이 무조건 켠다!
        if (healthBarUI != null && !healthBarUI.activeSelf) 
        {
            healthBarUI.SetActive(true);
        }
        
        currentHealth -= damage;
        if (healthSlider != null) healthSlider.value = currentHealth;

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        if (healthBarUI != null) healthBarUI.SetActive(false);
        Destroy(gameObject, 2.0f);
    }
}
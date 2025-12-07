using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Slider 사용을 위해 추가

public class EnemyHealth : MonoBehaviour
{
    // 체력 바 슬라이더를 연결할 변수
    public Slider healthSlider;

    // 최대 체력 (두 발에 죽도록 100 설정)
    public float maxHealth = 100f;
    private float currentHealth;

    // 총알의 데미지 상수 (한 발당 50)
    private const float BULLET_DAMAGE = 50f;

    // 시각 효과 변수
    private Renderer enemyRenderer;
    private Color originalColor;
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

    void Start()
    {
        // 1. 현재 체력을 최대 체력으로 초기화
        currentHealth = maxHealth;

        // 2. 체력 바 Slider 초기화 로직 (새로 추가/수정)
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth; // 슬라이더의 최대값을 설정
            healthSlider.value = currentHealth; // 슬라이더의 현재 값을 최대치로 설정 (가득 참)
        }

        // 3. 렌더러 초기화 로직 (기존 코드)
        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
    }

    /// <summary>
    /// 외부에서 데미지를 입힐 때 호출하는 함수입니다.
    /// </summary>
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log("Enemy took damage. Current Health: " + currentHealth);

        // 4. (추가) 데미지를 입을 때마다 Slider 값 업데이트
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        // 피격 시 색상 깜빡임 효과 시작
        if (enemyRenderer != null && currentHealth > 0)
        {
            StartCoroutine(FlashColor());
        }

        // 체력이 0 이하가 되면 Die 함수 호출
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 적이 총알(Bullet)에 맞았을 때 데미지를 처리하는 함수입니다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 1. 충돌한 오브젝트가 "Bullet" 태그를 가졌는지 확인
        if (other.CompareTag("Bullet"))
        {
            // 2. 데미지 적용
            TakeDamage(BULLET_DAMAGE);

            // 3. (필수) 총알 오브젝트 파괴
            Destroy(other.gameObject);
        }
    }

    private IEnumerator FlashColor()
    {
        enemyRenderer.material.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        enemyRenderer.material.color = originalColor;
    }

    /// <summary>
    /// 적이 사망했을 때 처리하는 로직입니다.
    /// </summary>
    void Die()
    {
        Debug.Log("Enemy Died!");

        // 적 오브젝트를 씬에서 파괴합니다.
        Destroy(gameObject);
    }
}
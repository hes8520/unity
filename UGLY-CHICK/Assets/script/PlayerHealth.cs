using UnityEngine;
using UnityEngine.UI;
using System.Collections; // 코루틴(쿨타임) 사용을 위해 추가

public class HealthBar : MonoBehaviour
{
    public Slider slider;

    public float maxHealth = 100f;
    private float currentHealth;

    private const float ENEMY_DAMAGE = 20f;

    // 쿨타임 변수
    private bool canTakeDamage = true; // 현재 데미지를 받을 수 있는지 여부
    public float invincibilityTime = 0.5f; // 무적 시간 (0.5초 설정)

    void Start()
    {
        currentHealth = maxHealth;
        slider.maxValue = maxHealth;
        slider.value = currentHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        slider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 접촉 유지 시 지속 피해 감지
    private void OnTriggerStay(Collider other)
    {
        // "sword1"과 접촉 중이며, 현재 데미지를 받을 수 있는 상태인지 확인
        if (other.gameObject.name == "sword1" && canTakeDamage)
        {
            TakeDamage(ENEMY_DAMAGE);

            // 데미지를 입힌 후 쿨타임 코루틴 시작
            StartCoroutine(DamageCooldown());
        }
    }

    // 쿨타임을 관리하는 코루틴
    private IEnumerator DamageCooldown()
    {
        canTakeDamage = false; // 무적 시작
        yield return new WaitForSeconds(invincibilityTime); // 설정된 시간만큼 대기
        canTakeDamage = true; // 무적 해제
    }

    void Die()
    {
        Debug.Log("Player Died! Game Over!");

        // 1. 플레이어 오브젝트 비활성화
        gameObject.SetActive(false);

        // 2. UIManager 관련 코드를 제거/주석 처리했습니다.
        // UIManager 클래스 정의 오류를 해결한 후 다시 추가해야 합니다.

        // if (UIManager.instance != null)
        // {
        //     UIManager.instance.SetActiveGameoverUI(true);
        // }
    }

    void LateUpdate()
    {

    }
}
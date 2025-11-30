using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 10;

    // Is Trigger가 체크되어 있어야 합니다!
    void OnTriggerEnter(Collider other)
    {
        // ★ 무엇과 부딪혔는지 콘솔에 범인을 출력합니다.
        Debug.Log("총알이 부딪힌 물체: " + other.name);

        BossHealth boss = other.GetComponent<BossHealth>();
        if (boss == null)
        {
            boss = other.GetComponentInParent<BossHealth>();
        }

        if (boss != null)
        {
            Debug.Log(">> 보스 감지 성공! 데미지 줌");
            boss.TakeDamage(damage);
            Destroy(gameObject);
        }
        else
        {
            // 보스가 아닌데 부딪혔다면?
            // 플레이어 몸이면 무시, 아니면 삭제
            if (other.gameObject.CompareTag("Player"))
            {
                Debug.Log(">> 내 몸에 맞음 (무시)");
                return; // 내 몸이면 그냥 통과
            }
            
            // 벽이나 바닥이면 삭제
            Debug.Log(">> 벽이나 장애물에 맞음");
            Destroy(gameObject);
        }
    }
}
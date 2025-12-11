using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("설정")]
    public float magnetRange = 3.0f; 
    public float moveSpeed = 10f;    
    
    // 아이템 1개당 물약 몇 개 줄지 (보통 1개)
    public int itemAmount = 1; 

    private Transform player;
    private bool isMagnetized = false; 

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) 
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < magnetRange) isMagnetized = true;

        if (isMagnetized)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

            if (distance < 0.5f)
            {
                GetItem();
            }
        }
    }

    void GetItem()
    {
        Debug.Log("힐템 획득!");

        // ★ [수정됨] 퀵슬롯 매니저에게 "물약(Potion)" 추가하라고 명령
        if (QuickSlotManager.Instance != null)
        {
            QuickSlotManager.Instance.AddPotion(itemAmount);
        }
        
        Destroy(gameObject); 
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }
}
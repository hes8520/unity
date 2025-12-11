using UnityEngine;

public class Bonfire : MonoBehaviour
{
    public string bonfireName = "지역 이름";
    public Transform spawnPoint; // ★이동할 위치(빈 오브젝트)
    private bool isNearby = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        { 
            isNearby = true; 
            BonfireManager.Instance.RegisterBonfire(this); 
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) 
        { 
            isNearby = false; 
            BonfireManager.Instance.CloseMenu(); 
        }
    }

    void Update()
    {
        if (isNearby && Input.GetKeyDown(KeyCode.F))
        {
            // 토글 기능 (켜져있으면 끄고, 꺼져있으면 켬)
            if (BonfireManager.Instance.bonfireWindow.activeSelf)
                BonfireManager.Instance.CloseMenu();
            else
                BonfireManager.Instance.OpenMenu();
        }
    }
}
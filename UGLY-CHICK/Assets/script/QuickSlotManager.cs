using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class WeaponItem
{
    public string weaponName;
    public Sprite icon;
    public GameObject prefab; 
}

public class QuickSlotManager : MonoBehaviour
{
    public static QuickSlotManager Instance;

    [Header("UI 연결")]
    public Image weaponIcon;    
    public Image potionIcon;    
    public TextMeshProUGUI potionCountText; 
    public TextMeshProUGUI ammoCountText;   

    [Header("장비 설정")]
    public Transform weaponHolder; 
    public List<WeaponItem> myWeapons; 
    
    // 물약 개수 (확인하기 쉽게 public)
    public int currentPotionCount = 5; 
    
    private int currentWeaponIndex = 0;
    private GameObject currentWeaponModel; 

    void Awake() { if (Instance == null) Instance = this; }

    void Start() { EquipWeapon(0); UpdatePotionUI(); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) SwapNextWeapon();
        
        // ★ 아래 화살표 누르면 물약 사용
        if (Input.GetKeyDown(KeyCode.DownArrow)) UsePotion();
    }

    public void UpdateAmmoText(int current, int max)
    {
        if (ammoCountText != null) ammoCountText.text = $"{current} / {max}";
    }

    // ★ [추가] 물약 획득 함수 (ItemPickup에서 호출)
    public void AddPotion(int amount)
    {
        currentPotionCount += amount;
        UpdatePotionUI();
        Debug.Log($"물약 획득! 현재 개수: {currentPotionCount}");
    }

    // ★ [수정] 물약 사용 함수 (체력 회복 기능 추가)
    void UsePotion()
    {
        if (currentPotionCount > 0)
        {
            // 1. 개수 줄이기
            currentPotionCount--;
            UpdatePotionUI();

            // 2. 플레이어 체력 회복 시키기 (30만큼)
            if (HealthBar.Instance != null)
            {
                HealthBar.Instance.Heal(30f);
            }
            else
            {
                Debug.LogWarning("플레이어를 찾을 수 없어서 회복 실패!");
            }
        }
        else
        {
            Debug.Log("물약이 부족합니다!");
        }
    }

    void SwapNextWeapon()
    {
        if (myWeapons.Count == 0) return;
        currentWeaponIndex = (currentWeaponIndex + 1) % myWeapons.Count;
        EquipWeapon(currentWeaponIndex);
    }

    void EquipWeapon(int index)
    {
        if (currentWeaponModel != null) Destroy(currentWeaponModel);
        
        WeaponItem item = myWeapons[index];
        if (item.prefab != null)
        {
            currentWeaponModel = Instantiate(item.prefab, weaponHolder);
            currentWeaponModel.transform.localPosition = Vector3.zero;
            currentWeaponModel.transform.localRotation = Quaternion.identity;
        }
        if (weaponIcon != null) weaponIcon.sprite = item.icon;
    }

    void UpdatePotionUI()
    {
        if (potionCountText != null) potionCountText.text = currentPotionCount.ToString();
    }
}
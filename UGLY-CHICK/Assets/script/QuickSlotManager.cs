using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // ★ 씬 관리 기능 추가

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

    [Header("UI 연결 (자동 탐색 사용시 이름 주의)")]
    public Image weaponIcon;    
    public Image potionIcon;    
    public TextMeshProUGUI potionCountText; 
    public TextMeshProUGUI ammoCountText;   

    [Header("장비 설정")]
    public Transform weaponHolder; 
    public List<WeaponItem> myWeapons; 
    
    // 물약 개수
    public int currentPotionCount = 5; 
    
    private int currentWeaponIndex = 0;
    private GameObject currentWeaponModel; 

    void Awake() 
    { 
        // ★ 싱글톤 유지 및 씬 전환 시 파괴 방지
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 이 오브젝트를 파괴하지 않음
        }
        else 
        {
            Destroy(gameObject); // 중복 생성 방지
        }
    }

    void Start() 
    { 
        EquipWeapon(0); 
        UpdatePotionUI(); 
    }

    // ★ 씬이 로드될 때마다 호출되는 이벤트 등록
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ★ 씬 로드 완료 시 실행: 끊어진 UI 다시 연결하기
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. 하이라키에서 이름으로 물약 텍스트 찾기
        // (주의: 유니티 에디터에서 텍스트 오브젝트 이름을 "PotionCountText"로 설정해주세요)
        GameObject potionObj = GameObject.Find("PotionCountText");
        if (potionObj != null)
        {
            potionCountText = potionObj.GetComponent<TextMeshProUGUI>();
        }

        // 2. 총알 텍스트도 필요하면 찾기 (이름: "AmmoCountText" 가정)
        GameObject ammoObj = GameObject.Find("AmmoCountText");
        if (ammoObj != null)
        {
            ammoCountText = ammoObj.GetComponent<TextMeshProUGUI>();
        }
        
        // 3. 찾은 UI에 현재 데이터 갱신
        UpdatePotionUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) SwapNextWeapon();
        
        // 아래 화살표 누르면 물약 사용
        if (Input.GetKeyDown(KeyCode.DownArrow)) UsePotion();
    }

    public void UpdateAmmoText(int current, int max)
    {
        if (ammoCountText != null) ammoCountText.text = $"{current} / {max}";
    }

    // 물약 획득 함수
    public void AddPotion(int amount)
    {
        currentPotionCount += amount;
        UpdatePotionUI();
        Debug.Log($"물약 획득! 현재 개수: {currentPotionCount}");
    }

    // 물약 사용 함수
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
                // 씬이 바뀌면 HealthBar 인스턴스도 바뀔 수 있으므로 다시 찾기 시도
                HealthBar foundHealth = FindObjectOfType<HealthBar>();
                if (foundHealth != null)
                {
                    foundHealth.Heal(30f);
                }
                else
                {
                    Debug.LogWarning("플레이어(HealthBar)를 찾을 수 없어서 회복 실패!");
                }
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
        if (potionCountText != null) 
        {
            potionCountText.text = currentPotionCount.ToString();
        }
    }
}
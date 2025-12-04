using UnityEngine;
using System.Collections;
using TMPro;

public class GunController : MonoBehaviour
{
    [Header("--- 필수 연결 ---")]
    public LockOnSystem lockOnSystem;    
    public TextMeshProUGUI ammoText;     

    [Header("--- 회전 보정 (중요) ---")]
    [Tooltip("캐릭터가 락온 시 옆을 본다면 조절 (예: -90, 90)")]
    public float modelRotationOffset = 0f; 

    [Tooltip("★ 총알이 옆으로 나간다면 이 값을 조절하세요! (예: -90, 90)")]
    public float bulletRotationOffset = 0f; 

    [Tooltip("★ 머즐 이펙트가 돌아가 있다면 이 값으로 돌려주세요 (x, y, z) - 보통 (90, 0, 0) 추천")]
    public Vector3 muzzleFlashRotationOffset; // [추가] 이펙트 각도 보정용 변수

    [Header("--- 총알 및 발사 설정 ---")]
    public GameObject bulletPrefab;      
    public Transform firePoint;          
    public float bulletSpeed = 50f;      
    public float shotDelay = 0.2f;       
    private float timeLastFired;         

    [Header("--- 재장전 설정 ---")]
    public int maxAmmo = 20;             
    public int currentAmmo;              
    public float reloadTime = 1.5f;      
    public bool isReloading = false;     
    public AudioClip reloadSound;        

    [Header("--- 이펙트 및 사운드 ---")]
    public GameObject muzzleFlashPrefab; 
    public AudioClip fireSound;          
    public AudioSource audioSource;      
    public Vector2 audioPitch = new Vector2(0.9f, 1.1f); 

    private PlayerMovement playerMovement;

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        playerMovement = GetComponentInParent<PlayerMovement>();
        if (lockOnSystem == null) lockOnSystem = GetComponentInParent<LockOnSystem>();

        timeLastFired = 0;
        currentAmmo = maxAmmo;
        
        UpdateAmmoUI();
    }

    void Update()
    {
        // 1. 락온 시 캐릭터 회전 (모델 보정 포함)
        if (lockOnSystem != null && lockOnSystem.isLockedOn && lockOnSystem.currentTarget != null)
        {
            if (playerMovement != null)
            {
                Vector3 dirToTarget = lockOnSystem.currentTarget.position - playerMovement.transform.position;
                dirToTarget.y = 0; 
                
                if (dirToTarget != Vector3.zero)
                {
                    Quaternion lookRot = Quaternion.LookRotation(dirToTarget);
                    // 캐릭터 몸체 회전 보정
                    Quaternion correctedRot = lookRot * Quaternion.Euler(0, modelRotationOffset, 0);
                    playerMovement.transform.rotation = correctedRot;
                }
            }
        }

        if (isReloading) return;

        // 재장전
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

        // 발사
        if (Input.GetMouseButton(0) && (Time.time >= timeLastFired + shotDelay))
        {
            if (playerMovement != null && playerMovement.isInvincible) return;
            if (currentAmmo <= 0) return; 

            FireWeapon();
        }
    }

    void FireWeapon()
    {
        timeLastFired = Time.time;
        currentAmmo--;
        UpdateAmmoUI();

        // 총구 방향에 '총알 보정 각도'를 더해서 최종 발사 방향을 계산
        Vector3 aimDir = Quaternion.Euler(0, bulletRotationOffset, 0) * firePoint.forward;

        // --- 총알 생성 ---
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            
            // 총알도 보정된 방향을 보게 함
            bullet.transform.forward = aimDir;

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(aimDir * bulletSpeed, ForceMode.Impulse);
            }
            
            Destroy(bullet, 3.0f);
        }

        // --- 이펙트 (수정됨) ---
        if (muzzleFlashPrefab != null && firePoint != null)
        {
            // [수정] 기존 발사 방향(LookRotation)에 보정 각도(Euler)를 곱해줍니다.
            Quaternion flashRotation = Quaternion.LookRotation(aimDir) * Quaternion.Euler(muzzleFlashRotationOffset);

            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, flashRotation, firePoint);
            Destroy(flash, 0.5f); 
        }

        // --- 사운드 ---
        if (audioSource != null && fireSound != null)
        {
            audioSource.pitch = Random.Range(audioPitch.x, audioPitch.y);
            audioSource.PlayOneShot(fireSound);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        if (ammoText != null) ammoText.text = "Reloading...";

        if (audioSource != null && reloadSound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + maxAmmo;
        }
    }
}
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;

public class GunController : MonoBehaviour
{
    [Header("--- 필수 연결 ---")]
    public LockOnSystem lockOnSystem;
    public TextMeshProUGUI ammoText;

    [Header("--- 회전 보정 (중요) ---")]
    public float modelRotationOffset = 0f;      // 캐릭터 모델 회전 보정
    public float bulletRotationOffset = 0f;     // 총알 발사 방향 보정
    public Vector3 muzzleFlashRotationOffset;   // 머즐 이펙트 보정

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
        if (audioSource == null) 
            audioSource = GetComponent<AudioSource>();

        playerMovement = GetComponentInParent<PlayerMovement>();
        if (lockOnSystem == null)
            lockOnSystem = GetComponentInParent<LockOnSystem>();

        timeLastFired = 0;
        currentAmmo = maxAmmo;
        
        UpdateAmmoUI();
        UpdateQuickSlot();
    }

    void Update()
    {
        // --- ★ UI 위에 마우스 있을 때는 발사 금지 ---
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // --- ★ 락온 상태일 때 캐릭터 회전 ---
        if (lockOnSystem != null && lockOnSystem.isLockedOn && lockOnSystem.currentTarget != null)
        {
            if (playerMovement != null)
            {
                Vector3 dir = lockOnSystem.currentTarget.position - playerMovement.transform.position;
                dir.y = 0;

                if (dir != Vector3.zero)
                {
                    Quaternion lookRot = Quaternion.LookRotation(dir);
                    Quaternion correctedRot = lookRot * Quaternion.Euler(0, modelRotationOffset, 0);
                    playerMovement.transform.rotation = correctedRot;
                }
            }
        }

        if (isReloading) return;

        // --- ★ 재장전 ---
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

        // --- ★ 발사 ---
        if (Input.GetMouseButton(0) && Time.time >= timeLastFired + shotDelay)
        {
            if (playerMovement != null && playerMovement.isInvincible) return; // 구르는 중이면 발사 X
            if (currentAmmo <= 0) return;

            FireWeapon();
        }
    }

    void FireWeapon()
    {
        timeLastFired = Time.time;
        currentAmmo--;
        UpdateAmmoUI();
        UpdateQuickSlot();

        // 총알 방향 보정
        Vector3 aimDir = Quaternion.Euler(0, bulletRotationOffset, 0) * firePoint.forward;

        // --- 총알 생성 ---
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bullet.transform.forward = aimDir;

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(aimDir * bulletSpeed, ForceMode.Impulse);
            }

            Destroy(bullet, 3f);
        }

        // --- 머즐 플래시 ---
        if (muzzleFlashPrefab != null && firePoint != null)
        {
            Quaternion flashRot = Quaternion.LookRotation(aimDir) * Quaternion.Euler(muzzleFlashRotationOffset);
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, flashRot, firePoint);
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
        if (ammoText != null)
            ammoText.text = "Reloading...";

        if (audioSource != null && reloadSound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;

        UpdateAmmoUI();
        UpdateQuickSlot();
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = currentAmmo + " / " + maxAmmo;
    }

    void UpdateQuickSlot()
    {
        if (QuickSlotManager.Instance != null)
            QuickSlotManager.Instance.UpdateAmmoText(currentAmmo, maxAmmo);
    }
}

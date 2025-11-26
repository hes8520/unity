using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("--- 총알 및 발사 설정 ---")]
    public GameObject bulletPrefab;      
    public Transform firePoint;          
    public float bulletSpeed = 20f;      
    public float shotDelay = 0.2f;       
    private float timeLastFired;         

    [Header("--- 이펙트 및 사운드 ---")]
    public GameObject muzzleFlashPrefab; 
    public AudioClip fireSound;          
    public AudioSource audioSource;      
    
    [Tooltip("소리의 높낮이를 랜덤하게 조절해 자연스럽게 만듭니다 (최소, 최대)")]
    public Vector2 audioPitch = new Vector2(0.9f, 1.1f); 

    // ★ 추가된 부분 1: 플레이어 움직임 스크립트를 가져올 변수
    private PlayerMovement playerMovement;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        timeLastFired = 0;

        // ★ 추가된 부분 2: 내 부모(Player)나 나한테 붙어있는 움직임 스크립트 찾기
        // (총은 보통 플레이어의 자식이므로 InParent로 찾습니다)
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    void Update()
    {
        // 마우스 클릭 감지 + 쿨타임 체크
        if (Input.GetMouseButton(0) && (Time.time >= timeLastFired + shotDelay))
        {
            // ★ 추가된 부분 3: 구르는 중(무적 상태)이면 발사 금지!
            if (playerMovement != null && playerMovement.isInvincible)
            {
                return; // 여기서 코드 실행을 멈춰서 FireWeapon으로 못 가게 함
            }

            FireWeapon();
        }
    }

    void FireWeapon()
    {
        // --- 1. 발사 시간 갱신 ---
        timeLastFired = Time.time;

        // --- 2. 총알 생성 및 발사 ---
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(firePoint.forward * bulletSpeed, ForceMode.Impulse);
            }
            
            Destroy(bullet, 3.0f);
        }

        // --- 3. 총구 이펙트 (Muzzle Flash) ---
        if (muzzleFlashPrefab != null && firePoint != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);
            Destroy(flash, 0.5f); 
        }

        // --- 4. 사운드 재생 (랜덤 피치 적용) ---
        if (audioSource != null && fireSound != null)
        {
            audioSource.pitch = Random.Range(audioPitch.x, audioPitch.y);
            audioSource.PlayOneShot(fireSound);
        }
    }
}
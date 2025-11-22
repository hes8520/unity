using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("--- 총알 및 발사 설정 ---")]
    public GameObject bulletPrefab;      // 총알 프리팹
    public Transform firePoint;          // 발사 위치 (MuzzlePosition)
    public float bulletSpeed = 20f;      // 총알 속도
    public float shotDelay = 0.2f;       // 발사 간격 (초 단위, 낮을수록 빠름)
    private float timeLastFired;         // 마지막 발사 시간 기억용

    [Header("--- 이펙트 및 사운드 ---")]
    public GameObject muzzleFlashPrefab; // 총구 화염 이펙트
    public AudioClip fireSound;          // 발사 소리
    public AudioSource audioSource;      // 오디오 소스 컴포넌트
    
    [Tooltip("소리의 높낮이를 랜덤하게 조절해 자연스럽게 만듭니다 (최소, 최대)")]
    public Vector2 audioPitch = new Vector2(0.9f, 1.1f); 

    void Start()
    {
        // 시작할 때 AudioSource가 연결 안 되어 있으면 자동으로 찾음
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        // 마지막 발사 시간 초기화
        timeLastFired = 0;
    }

    void Update()
    {
        // 1. 마우스 클릭 감지
        // 2. 현재 시간이 (마지막 발사시간 + 딜레이)보다 컸는지 확인 (쿨타임 체크)
        if (Input.GetMouseButton(0) && (Time.time >= timeLastFired + shotDelay))
        {
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
            // 총알 생성 (부모를 설정하지 않음 -> 총알이 총을 따라다니지 않게 하기 위함)
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            
            // 물리 힘 가하기
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(firePoint.forward * bulletSpeed, ForceMode.Impulse);
            }
            
            // 3초 뒤 총알 삭제
            Destroy(bullet, 3.0f);
        }

        // --- 3. 총구 이펙트 (Muzzle Flash) ---
        if (muzzleFlashPrefab != null && firePoint != null)
        {
            // 총구 위치에 이펙트 생성, 총구의 자식으로 설정(총 따라가게)
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);
            Destroy(flash, 0.5f); // 0.5초 뒤 삭제
        }

        // --- 4. 사운드 재생 (랜덤 피치 적용) ---
        if (audioSource != null && fireSound != null)
        {
            // 소리 높낮이를 랜덤하게 바꿈 (기계적인 소리 방지)
            audioSource.pitch = Random.Range(audioPitch.x, audioPitch.y);
            // 소리 재생
            audioSource.PlayOneShot(fireSound);
        }
    }
}
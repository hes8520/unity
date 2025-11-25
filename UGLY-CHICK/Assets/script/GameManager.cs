using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 어디서든 접근 가능하게 함

    // 저장할 정보들
    public Vector3 savedPosition; // 돌아갈 위치
    public string sceneToReturn;  // 돌아갈 씬 이름 (원래 맵 이름)
    public bool isReturning = false; // 지금 돌아오는 길인가?
    public Quaternion savedRotation;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바껴도 파괴되지 않음!
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
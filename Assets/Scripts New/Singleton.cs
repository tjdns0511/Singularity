using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();

                if (_instance == null )
                {
                    Debug.LogError($"씬에 {typeof(T)} 타입의 인스턴스가 존재하지 않습니다.");
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// 자식 클래스가 원하는 로직 추가 가능하게 virtual로 선언
    /// </summary>
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Debug.LogWarning($"[Singleton] {typeof(T)}가 이미 존재하여 새 오브젝트 파괴");
            Destroy(gameObject);
        }
    }
}

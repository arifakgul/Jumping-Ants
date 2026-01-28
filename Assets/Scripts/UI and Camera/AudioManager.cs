using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance {get; private set;}

    [Header("UI Sounds")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioSource sfxSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayClickSound()
    {
        if (sfxSource != null && clickClip != null)
        {
            sfxSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(clickClip);
        }
    }
}

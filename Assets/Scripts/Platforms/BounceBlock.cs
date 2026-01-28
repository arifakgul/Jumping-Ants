using System.Collections;
using UnityEngine;

public class BounceBlock : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private float bounceVelocity = 14f;
    [SerializeField] private float cooldown = 0.05f; // aynı frame içinde birden çok tetiklemeyi engelle
    [SerializeField] private float animationDuration = 0.15f;

    [Header("Ses Ayarları")]
    [SerializeField] private AudioClip bounceClip; 
    [SerializeField] private float volume = 1f;

    private float lastBounceTime = -999f;
    private Vector3 originalScale;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        originalScale = transform.localScale;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;
        if (Time.time - lastBounceTime < cooldown) return;

        var rb = collision.collider.attachedRigidbody;
        if (rb == null) return;

        // Yalnızca oyuncu aşağı doğru gelirken tetikle
        if (rb.linearVelocity.y > 0f) return;

        Vector2 v = rb.linearVelocity;
        v.y = bounceVelocity;
        rb.linearVelocity = v;

        lastBounceTime = Time.time;

        PlayBounceSound();

        StopAllCoroutines();
        StartCoroutine(BounceAnimation());
    }

    private void PlayBounceSound()
    {
        if (audioSource != null && bounceClip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f); 
            audioSource.PlayOneShot(bounceClip, volume);
        }
    }

    private IEnumerator BounceAnimation()
    {
        Vector3 squashScale = new Vector3(originalScale.x * 1.2f, originalScale.y * 0.7f, originalScale.z);

        float timer = 0f;
        while (timer < animationDuration / 2)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, squashScale, timer / (animationDuration / 2));
            yield return null;
        }

        timer = 0f;
        while (timer < animationDuration / 2)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(squashScale, originalScale, timer / (animationDuration / 2));
            yield return null;
        }

        transform.localScale = originalScale;
    }
}

using UnityEngine;

public class UITextPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float speed = 3f; 
    [SerializeField] private float intensity = 0.05f; 

    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        float scaleFactor = 1f + Mathf.Sin(Time.unscaledTime * speed) * intensity;
        transform.localScale = initialScale * scaleFactor;
    }
}
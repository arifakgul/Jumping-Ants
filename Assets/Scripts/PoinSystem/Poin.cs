using UnityEngine;

public class Poin : MonoBehaviour
{
    private int coinValue = 1;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Collect();
            ObjectPool.Instance.ReturnToPool("Poin", gameObject);
        }
    }

    private void Collect()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPoin(coinValue);
        }
    }
}

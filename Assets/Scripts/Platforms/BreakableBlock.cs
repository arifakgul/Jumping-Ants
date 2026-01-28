using System.Collections;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    [SerializeField] private float destroyTime = 0.3f;
    private bool triggered = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (triggered) return;
        if (!collision.collider.CompareTag("Player")) return;

        if (collision.relativeVelocity.y <= 0f)
        {
            triggered = true;
            StartCoroutine(BreakRoutine());
        }
    }

    private IEnumerator BreakRoutine()
    {
        yield return new WaitForSeconds(destroyTime);
        Destroy(gameObject);
    }
}

using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float moveRange = 3;
    [SerializeField] private bool autoClampToScreen = true;

    private float leftLimit;
    private float rightLimit;
    private Vector3 startPos;
    private int direction = 1;

    void Start()
    {
        startPos = transform.position;

        if (autoClampToScreen)
        {
            Camera cam = Camera.main;
            Vector3 leftScreen = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
            Vector3 rightScreen = cam.ViewportToWorldPoint(new Vector3(1, 0, 0));

            float halfWidth = GetComponent<SpriteRenderer>() != null
                ? GetComponent<SpriteRenderer>().bounds.extents.x
                : 0.5f;

            leftLimit = leftScreen.x + halfWidth;
            rightLimit = rightScreen.x - halfWidth;
        }
        else
        {
            leftLimit = startPos.x - moveRange;
            rightLimit = startPos.x + moveRange;
        }
    }

    void FixedUpdate()
    {
        MovePlatform();
    }

    private void MovePlatform()
    {
        transform.Translate(Vector2.right * direction * moveSpeed * Time.fixedDeltaTime);
        if (transform.position.x >= rightLimit)
        {
            direction = -1;
            transform.position = new Vector2(rightLimit, transform.position.y);
        }
        else if (transform.position.x <= leftLimit)
        {
            direction = 1;
            transform.position = new Vector2(leftLimit, transform.position.y);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (!Application.isPlaying)
        {
            float halfRange = moveRange;
            Vector3 left = transform.position + Vector3.left * halfRange;
            Vector3 right = transform.position + Vector3.right * halfRange;
            Gizmos.DrawLine(left, right);
            Gizmos.DrawSphere(left, 0.05f);
            Gizmos.DrawSphere(right, 0.05f);
        }
        else
        {
            Gizmos.DrawLine(new Vector3(leftLimit, transform.position.y),
                            new Vector3(rightLimit, transform.position.y));
        }
    }
}
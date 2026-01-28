using UnityEngine;

public class Meteor : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 12f;
    private Camera mainCam;
    private ObjectPool pool;

    private void Awake()
    {
        mainCam = Camera.main;
        pool = ObjectPool.Instance;
    }

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        if (transform.position.y < mainCam.transform.position.y - 10f)
        {
            pool.ReturnToPool("Meteor", gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.Die();
            }
            pool.ReturnToPool("Meteor", gameObject);
        }
        else if (collision.gameObject.CompareTag("Platform"))
        {
            // platformu yok etme isteği ileride buraya koyabilirim.
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
        }
    }
}

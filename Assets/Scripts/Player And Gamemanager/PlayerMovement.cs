using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float defaultJumpForce = 11.5f;

    [Header("Lane Settings")]
    [SerializeField] private float laneDistance = 2f;
    [SerializeField] private float laneSwitchSpeed = 20f;
    [SerializeField] private float laneSmoothTime = 0.05f;
    [SerializeField] private bool useLerpTransition = false;

    [Header("Ground / Platform Detection")]
    [SerializeField] private float groundCheckRadius = 0.09f;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask plaLayerMask;
    [SerializeField] private Button button;


    private Rigidbody2D rb;
    private SpriteRenderer playerRenderer;

    private bool isGrounded = true;
    private bool jumpRequest = false;

    private int currentLane = 1;
    private Vector2 velocitySmooth = Vector2.zero;

    private Vector2 touchStartPos; 
    private float minSwipeDistance;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerRenderer = GetComponent<SpriteRenderer>();
        minSwipeDistance = Screen.height * 0.05f;
    }

    void Update()
    {
        TestInput();
        TouchInput();
        CheckGrounded();
        CheckDie();
    }

    void FixedUpdate()
    {
        LaneSwitch();
        HandleJumping();
    }

    // ----------- Input / Kontrol / Lane Switch -------------
    private void TestInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) jumpRequest = true;
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLane(1);
            playerRenderer.flipX = false;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLane(-1);
            playerRenderer.flipX = true;
        }
    }

    private void TouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            touchStartPos = touch.position;
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            Vector2 delta = touch.position - touchStartPos;
            if (delta.magnitude < minSwipeDistance) return;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (delta.x > 0)
                {
                    ChangeLane(1);
                    playerRenderer.flipX = false;
                }
                else
                {
                    ChangeLane(-1);
                    playerRenderer.flipX = true;
                }
            }
            else
            {
                if (delta.y > 0 && isGrounded) jumpRequest = true;
            }
        }
    }

    private void ChangeLane(int lan)
    {
        int target = Mathf.Clamp(currentLane + lan, 0, 2);
        if (target != currentLane)
        {
            currentLane = target;
        }
    }

    private void LaneSwitch()
    {
        float targetX = (currentLane - 1) * laneDistance;
        Vector2 targetPos = new Vector2(targetX, rb.position.y);

        Vector2 newPos;
        if (useLerpTransition)
        {
            newPos = Vector2.Lerp(rb.position, targetPos, Time.fixedDeltaTime * 24f);
        }
        else
        {
            newPos = Vector2.SmoothDamp(rb.position, targetPos, ref velocitySmooth, laneSmoothTime, laneSwitchSpeed);
        }
        transform.position = Vector3.MoveTowards(transform.position, newPos, laneSwitchSpeed * Time.deltaTime);
    }

    // ----------- Jump / Ground Detection -------------
    private void HandleJumping()
    {
        if (jumpRequest && isGrounded)
        {
            ApplyJump(defaultJumpForce);
            jumpRequest = false;
        }
    }

    private void ApplyJump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        isGrounded = false;
    }

    private void CheckGrounded()
    {
        if (groundCheckPoint != null)
        {
            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, plaLayerMask);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }

    public void ToggleUseLerp()
    {
        useLerpTransition = !useLerpTransition;
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = useLerpTransition ? Color.green : Color.white;
            colors.selectedColor = colors.normalColor;
            colors.pressedColor = colors.normalColor * 0.9f;
            button.colors = colors;
        }
    }

    private void CheckDie()
    {
        if (Camera.main == null) return;
        if (transform.position.y < Camera.main.transform.position.y - 6f)
        {
            Die();
        }
    }

    public void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        enabled = false;
        playerRenderer.flipY = true;
        rb.linearVelocity = Vector2.zero;
    }
}
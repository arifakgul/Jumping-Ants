// PlayerMovement.cs
/*
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
public class Depo : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 11.5f;
    public bool autoJump = true;

    [Header("Lane Settings")]
    public float laneDistance = 2f;
    public float laneSwitchSpeed = 10f;


    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool jumpRequest = false;
    private int currentLane = 1;

    private Vector2 touchStartPos, touchEndPos;
    private float minSwipeDistance;
    private Vector3[] lanes = new Vector3[3];


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        minSwipeDistance = Screen.height * 0.05f;

        lanes[0] = new Vector3(-laneDistance, transform.position.y, 0);
        lanes[1] = new Vector3(0, transform.position.y, 0);
        lanes[2] = new Vector3(laneDistance, transform.position.y, 0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
            jumpRequest = true;
        if (Input.GetKeyDown(KeyCode.RightArrow)) MoveRight();
        if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveLeft();

        DetectSwipe();

        //if (autoJump && isGrounded) Jump();
    }

    void FixedUpdate()
    {
        if (jumpRequest)
        {
            ExecuteJump();
            jumpRequest = false;
        }

        MoveToLane();


    }

    private void ExecuteJump()
    {
        if (!isGrounded) return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isGrounded = false;
    }

    public void Jump()
    {
        if (isGrounded) jumpRequest = true;
    }

    private void DetectSwipe()
    {
        if (Input.touchCount == 0) return;

        else if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                touchEndPos = touch.position;
                Vector2 swipeDelta = touchEndPos - touchStartPos;

                if (swipeDelta.magnitude >= minSwipeDistance)
                {
                    float x = swipeDelta.x;
                    float y = swipeDelta.y;

                    if (Mathf.Abs(x) > Mathf.Abs(y))
                    {
                        if (x > 0)
                        {
                            MoveRight();
                        }
                        else
                        {
                            MoveLeft();
                        }
                    }
                    else
                    {
                        if (y > 0 && !autoJump && isGrounded)
                            jumpRequest = true;
                    }
                }
            }
        }
    }

    private void MoveRight()
    {
        if (currentLane < 2) currentLane += 1;
    }

    private void MoveLeft()
    {
        if (currentLane > 0) currentLane -= 1;
    }

    private void MoveToLane()
    {
        Vector3 targetPos = new Vector3(lanes[currentLane].x, transform.position.y, 0);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, laneSwitchSpeed * Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = true;
        }

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            if (contact.normal.y > 0.5f && rb.linearVelocity.y <= 0f)
            {
                if (autoJump) jumpRequest = true;
                break;
            }
        }
    }
}
*/

// 2.Version
/*
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] public bool autoJump = true;

    [Header("Lane Settings")]
    [SerializeField] private float laneDistance = 2f;
    [SerializeField] private float laneSwitchDuration = 0.12f;

    private Rigidbody2D rb;
    private int currentLane = 1;
    private bool isGrounded = false;
    private bool jumpRequest = false;
    private bool isSwitching = false;
    private Queue<int> laneQueue = new Queue<int>();

    private Vector2 touchStartPos;
    private float minSwipeDistance;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; //fizik tabanlı hareketler için yumuşak görüntü düşük fps de de iş yapar.

        minSwipeDistance = Screen.height * 0.03f;
    }

    void Update()
    {
        TestInput();
        TouchInput();
    }

    void FixedUpdate()
    {
        if (jumpRequest)
        {
            ExecuteJump();
            jumpRequest = false;
        }
    }

    #region Test Input
    private void TestInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded) jumpRequest = true;
        if (Input.GetKeyDown(KeyCode.RightArrow)) QueueLane(currentLane + 1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) QueueLane(currentLane - 1);
    }

    private void TouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        switch (touch.phase)
        {
            case TouchPhase.Began:
                touchStartPos = touch.position;
                break;
            case TouchPhase.Ended:
                HandleSwipe(touch.position - touchStartPos);
                break;
        }
    }

    private void HandleSwipe(Vector2 swipeDelta)
    {
        if (swipeDelta.magnitude < minSwipeDistance) return;

        if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
        {
            if (swipeDelta.x > 0) QueueLane(currentLane + 1);
            else QueueLane(currentLane - 1);
        }
        else if (swipeDelta.y > 0 && !autoJump && isGrounded)
        {
            jumpRequest = true;
        }
    }
    #endregion

    #region Jump
    private void ExecuteJump()
    {
        if (!isGrounded) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isGrounded = false;
    }

    public void Jump()
    {
        if (isGrounded) jumpRequest = true;
        else return;
    }
    #endregion

    #region Lane Switching
    private void QueueLane(int targetLane)
    {
        targetLane = Mathf.Clamp(targetLane, 0, 2);
        if (targetLane != currentLane)
        {
            laneQueue.Enqueue(targetLane);
        }
        if (!isSwitching) StartCoroutine(ProcessLaneQueue());
    }

    private IEnumerator ProcessLaneQueue()
    {
        while (laneQueue.Count > 0)
        {
            isSwitching = true;
            int nextLane = laneQueue.Dequeue();

            Vector3 startPos = transform.position;
            Vector3 targetPos = new Vector3((nextLane - 1) * laneDistance, transform.position.y, 0f);

            float elapsed = 0f;
            while (elapsed < laneSwitchDuration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, targetPos, elapsed / laneSwitchDuration);
                yield return null;
            }

            transform.position = targetPos;
            currentLane = nextLane;
        }
        isSwitching = false;
    }
    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = true;
        }

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            if (contact.normal.y > 0.5f && rb.linearVelocity.y <= 0f)
            {
                if (autoJump) jumpRequest = true;
                break;
            }
        }    
    }

}
*/


/*
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 11.5f;
    public bool autoJump = true;

    [Header("Lane Settings")]
    public float laneDistance = 2f;
    public float laneSwitchDuration = 0.15f;


    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool jumpRequest = false;
    private bool isSwitching = false;

    private int currentLane = 1;
    private Vector3[] lanes = new Vector3[3];

    private Vector2 touchStartPos, touchEndPos;
    private float minSwipeDistance;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        minSwipeDistance = Screen.height * 0.05f;

        lanes[0] = new Vector3(-laneDistance, transform.position.y, 0);
        lanes[1] = new Vector3(0, transform.position.y, 0);
        lanes[2] = new Vector3(laneDistance, transform.position.y, 0);
    }

    void Update()
    {
        TestInput();
        DetectSwipe();

        //if (autoJump && isGrounded) Jump();
    }

    void FixedUpdate()
    {
        if (jumpRequest)
        {
            ExecuteJump();
            jumpRequest = false;
        }
    }

    private void TestInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded) jumpRequest = true;
        if (Input.GetKeyDown(KeyCode.RightArrow)) MoveRight();
        if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveLeft();
    }

    private void ExecuteJump()
    {
        if (!isGrounded) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isGrounded = false;
    }

    public void Jump()
    {
        if (isGrounded) jumpRequest = true;
    }

    private void DetectSwipe()
    {
        if (Input.touchCount == 0) return;

        else if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                touchEndPos = touch.position;
                Vector2 swipeDelta = touchEndPos - touchStartPos;

                if (swipeDelta.magnitude >= minSwipeDistance)
                {
                    float x = swipeDelta.x;
                    float y = swipeDelta.y;

                    if (Mathf.Abs(x) > Mathf.Abs(y))
                    {
                        if (x > 0) MoveRight();
                        else MoveLeft();
                    }
                    else
                    {
                        if (y > 0 && !autoJump && isGrounded)
                            jumpRequest = true;
                    }
                }
            }
        }
    }

    private void MoveRight()
    {
        if (currentLane < 2 && !isSwitching)
        {
            currentLane++;
            StartCoroutine(SwitchLane());
        }
    }

    private void MoveLeft()
    {
        if (currentLane > 0 && !isSwitching)
        {
            currentLane--;
            StartCoroutine(SwitchLane());
        }
    }

    private IEnumerator SwitchLane()
    {
        isSwitching = true;

        float elapsed = 0f;
        float duration = laneSwitchDuration;

        float startX = rb.position.x;
        float targetX = lanes[currentLane].x;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            float newX = Mathf.Lerp(startX, targetX, t);

            rb.MovePosition(new Vector2(newX, rb.position.y));

            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(new Vector2(targetX, rb.position.y));
        isSwitching = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = true;
        }

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            if (contact.normal.y > 0.5f && rb.linearVelocity.y <= 0f)
            {
                if (autoJump) jumpRequest = true;
                break;
            }
        }    
    }
}
*/


//3.version
/*
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float defaultJumpForce = 11.5f;
    //[SerializeField] private float autoJumpMinDelay = 0.05f;
    //[SerializeField] private bool autoJump = false;

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

    private bool isGrounded = true;
    private bool jumpRequest = false;

    private int currentLane = 1;
    private Vector2 velocitySmooth = Vector2.zero;

    private Vector2 touchStartPos;
    private float minSwipeDistance;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        minSwipeDistance = Screen.height * 0.05f;
    }

    void Update()
    {
        TestInput();
        TouchInput();
        CheckGrounded();
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
        if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeLane(1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeLane(-1);
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
                if (delta.x > 0) ChangeLane(1);
                else ChangeLane(-1);
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
        Debug.Log("useLerpTransition = " + useLerpTransition);
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = useLerpTransition ? Color.green : Color.white;
            colors.selectedColor = colors.normalColor;
            colors.pressedColor = colors.normalColor * 0.9f;
            button.colors = colors;
        }
    }
}
*/

//======================= Moving Platform =======================
/*
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Lane Settings")]
    public int[] lanes = { -1, 0, 1 };
    public float laneDistance = 2f;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float phaseOffset = 0f;
    public Vector3 Delta { get; private set; }

    private int currentLaneIndex = 0;
    private int targetLaneIndex = 1;
    private Vector3 targetPos;
    private Vector3 lastPos;
    
    void Start()
    {
        if (lanes == null || lanes.Length < 2)
        {
            enabled = false;
            return;
        }

        // İlk hedefi belirle
        targetPos = GetLaneWorldPos(lanes[targetLaneIndex]);
        lastPos = transform.position;
    }

    void FixedUpdate()
    {
        // Platformu hedef lane'e taşı
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // Delta (oyuncu taşımak için)
        Delta = transform.position - lastPos;
        lastPos = transform.position;

        // Hedefe ulaştığında yön değiştir
        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            currentLaneIndex = targetLaneIndex;

            // sıradaki lane'e geç
            if (targetLaneIndex + 1 < lanes.Length)
                targetLaneIndex++;
            else
                targetLaneIndex = 0;

            targetPos = GetLaneWorldPos(lanes[targetLaneIndex]);
        }
    }

    private Vector3 GetLaneWorldPos(int lane)
    {
        return new Vector3(lane * laneDistance, transform.position.y, 0f);
    }
}
*/

/*
using UnityEngine;

/// <summary>
/// Profesyonel, ekran sınırlarında sağa-sola hareket eden platform.
/// - Kamera sınırlarını otomatik algılar.
/// - Ekran dışına taşmaz.
/// - Herhangi bir yüksekliğe yerleştirilebilir.
/// - Yön değişimi yumuşak ve fizik dostu.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.5f;     // Platform hızı
    [SerializeField] private float moveRange = 3f;       // Platformun gidebileceği maksimum mesafe (opsiyonel)
    [SerializeField] private bool autoClampToScreen = true; // Ekran sınırlarını otomatik algıla
    [SerializeField] private bool pingPongMovement = false; // true: range bazlı hareket, false: ekran sınırları bazlı

    private float leftLimit;
    private float rightLimit;
    private Vector3 startPos;
    private int direction = 1; // 1 = sağa, -1 = sola

    void Start()
    {
        startPos = transform.position;

        if (autoClampToScreen)
        {
            // Ekran sınırlarını hesapla (kameraya göre)
            Camera cam = Camera.main;
            Vector3 leftScreen = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
            Vector3 rightScreen = cam.ViewportToWorldPoint(new Vector3(1, 0, 0));

            float halfWidth = GetComponent<SpriteRenderer>() != null
                ? GetComponent<SpriteRenderer>().bounds.extents.x
                : 0.5f; // Sprite yoksa tahmini yarıçap

            leftLimit = leftScreen.x + halfWidth;
            rightLimit = rightScreen.x - halfWidth;
        }
        else
        {
            // Eğer ekran sınırı istemiyorsan manuel aralık kullan
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
        // Yeni pozisyonu hesapla
        transform.Translate(Vector2.right * direction * moveSpeed * Time.fixedDeltaTime);

        // Sağ veya sol sınıra ulaştı mı?
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

    // Editor içinde görsel rehber
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
*/


//========================= platform spawner =========================
/*
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Lane Settings")]
    [SerializeField] private float laneDistance = 2f;
    private float[] lanes;

    [Header("Spawn Settings")]
    [SerializeField] private float verticalSpacing = 2f;
    [SerializeField] private float spawnAhead = 12f;
    [SerializeField] private float cleanupBehind = 10f;
    [SerializeField] private int initialRows = 7; //başlangıç blok sayısı 7 satır spawlanacak
    [SerializeField] private float spacingJitter = 0.2f; //bloklar arası mesafe için randomluk tam 2 değil 2.2 gibi bişey olcak

    [Header("Pool Tags")]
    [SerializeField] private string singleTag = "Single";
    [SerializeField] private string doubleTag = "Double";
    [SerializeField] private string bigTag = "Big";
    [SerializeField] private string breakableTag = "Breakable";
    [SerializeField] private string movingTag = "Moving";
    [SerializeField] private string bounceTag = "Bounce";

    private readonly List<GameObject> activeBlocks = new List<GameObject>();
    private float nextY;
    private int lastPattern = -1; //Bu, önceki seçilen pattern’i hatırlıyor. Sonraki seçimde aynı pattern (bucket + pattern) üst üste gelmesin diye kontrol ediliyor 

    float EasyWeight(float y) => Mathf.Clamp01(1f - (y / 180f));
    float MediumWeight(float y) => Mathf.Clamp01(0.2f + (y / 240f));
    float HardWeight(float y) => Mathf.Clamp01((y - 60f) / 180f);

    void Start()
    {
        lanes = new float[] { -laneDistance, 0, laneDistance };
        nextY = player.position.y + verticalSpacing;

        for (int i = 0; i < initialRows; i++)
        {
            SpawnUntilAhead();
        }
    }

    void Update()
    {
        SpawnUntilAhead();
        CleanUp();
    }

    private void SpawnUntilAhead()
    {
        while (player.position.y + spawnAhead > nextY)
        {
            SpawnRow(nextY);
            nextY += verticalSpacing + UnityEngine.Random.Range(-spacingJitter, spacingJitter);
        }
    }

    private void SpawnRow(float y)
    {
        if (Mathf.RoundToInt(y + spawnAhead) % 100 == 0)
        {
            SpawnBig(bigTag, y);
            return;
        }

        float a = EasyWeight(y);
        float r = MediumWeight(y);
        float o = HardWeight(y);

        float sum = a + r + o;
        if (sum == 0.001f)
        {
            a = 1; r = 0; o = 0;
            sum = 1;
        }
        a /= sum; r /= sum; o /= sum;

        int bucket = WeightedPick(new float[] { a, r, o });
        int pattern = PickPattern(bucket);

        switch (bucket)
        {
            case 0: SpawnEasyPattern(pattern, y); break;
            case 1: SpawnMediumPattern(pattern, y); break;
            case 2: SpawnHardPattern(pattern, y); break;
        }

        lastPattern = (bucket << 8) | pattern;
    }

    private void SpawnEasyPattern(int idx, float y)
    {
        switch (idx)
        {
            case 0: SpawnSingle(singleTag, RandLane(), y); break;
            case 1: SpawnDouble(doubleTag, UnityEngine.Random.value < 0.5f ? 0 : 1, y); break;
            default:
                int[] lanes3 = RandLanePermutation();
                SpawnSingle(breakableTag, lanes3[0], y);
                SpawnSingle(bounceTag, lanes3[1], y);
                SpawnSingle(singleTag, lanes3[2], y);
                break;  
        }
    }

    private void SpawnMediumPattern(int idx, float y)
    {
        switch (idx)
        {
            case 0:
                int a = RandLane();
                int b = RandOtherLane(a);
                SpawnSingle(singleTag, a, y);
                SpawnSingle(singleTag, b, y);
                break;
            case 1:
                int l1 = RandLane();
                int l2 = RandOtherLane(l1);
                SpawnSingle(singleTag, l1, y);
                SpawnSingle(bounceTag, l2, y);
                break;
            case 2:
                int s1 = RandLane();
                SpawnSingle(movingTag, s1, y);
                break;
            case 3:
                int[] lanes3 = RandLanePermutation();
                for (int i = 0; i < 3; i++)
                {
                    SpawnSingle(singleTag, lanes3[i], y + i * (verticalSpacing * 0.6f));
                }
                break;
            default:
                int m1 = RandLane();
                int m2 = RandOtherLane(m1);
                SpawnSingle(singleTag, m1, y);
                SpawnSingle(bounceTag, m2, y);
                break;
        }
    }

    private void SpawnHardPattern(int idx, float y)
    {
        switch (idx)
        {
            case 0:
                int[] lanes3 = RandLanePermutation();
                SpawnSingle(breakableTag, lanes3[0], y);
                SpawnSingle(bounceTag, lanes3[1], y);
                SpawnSingle(singleTag, lanes3[2], y);
                break;
            case 1:
                int start = UnityEngine.Random.value < 0.5f ? 0 : 1;
                SpawnDouble(doubleTag, start, y);
                int remain = (start == 0) ? 2 : 0;
                SpawnSingle(breakableTag, remain, y);
                break;
            case 2:
                int s3 = RandLane();
                SpawnSingle(movingTag, s3, y);
                break;
            default:
                int s1 = RandLane();
                int s2 = RandOtherLane(s1);
                SpawnSingle(breakableTag, s1, y);
                SpawnSingle(bounceTag, s2, y);
                break;
        }
    }

    private void SpawnSingle(string tag, int laneIndex, float y)
    {
        var go = ObjectPool.Instance.SpawnFromPool(tag, new Vector2(lanes[laneIndex], y), Quaternion.identity);
        activeBlocks.Add(go);
    }

    private void SpawnDouble(string tag, int startLane, float y)
    {
        float midX = 0.5f * (lanes[startLane] + lanes[startLane + 1]);
        var go = ObjectPool.Instance.SpawnFromPool(tag, new Vector2(midX, y), Quaternion.identity);
        activeBlocks.Add(go);
    }

    private void SpawnBig(string tag, float y)
    {
        var go = ObjectPool.Instance.SpawnFromPool(tag, new Vector2(0f, y), Quaternion.identity);
        activeBlocks.Add(go);
    }

    int RandLane() => UnityEngine.Random.Range(0, 3);
    int RandOtherLane(int a)
    {
        int b = UnityEngine.Random.Range(0, 3);
        while (b == a) b = UnityEngine.Random.Range(0, 3);
        return b;
    }
    int[] RandLanePermutation()
    {
        var list = new List<int> { 0, 1, 2 };
        for (int i = 0; i < 3; i++)
        {
            int r = UnityEngine.Random.Range(i, 3);
            (list[i], list[r]) = (list[r], list[i]);
        }
        return list.ToArray();
    }
    int WeightedPick(float[] weights)
    {
        float sum = 0f;
        foreach (var w in weights) sum += w;
        float r = UnityEngine.Random.value * sum, c = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            c += weights[i];
            if (r <= c) return i;
        }
        return weights.Length - 1;
    }
    int PickPattern(int bucket)
    {
        int candidate;
        int guard = 0;
        do
        {
            candidate = UnityEngine.Random.Range(0, 3);
            guard++;
        } while (((bucket << 8) | candidate) == lastPattern && guard < 6);
        return candidate;
    }

    private void CleanUp()
    {
        for (int i = activeBlocks.Count - 1; i >= 0; i--)
        {
            var g = activeBlocks[i];
            if (g == null) { activeBlocks.RemoveAt(i); continue; }

            if (g.transform.position.y < player.position.y - cleanupBehind)
            {
                // Destroy yerine pool’a iade
                string tag = g.name.Replace("(Clone)", "").Trim();
                ObjectPool.Instance.ReturnToPool(tag, g);
                activeBlocks.RemoveAt(i);
            }
        }
    }
}
*/

// ===================== MeteorManager =====================
/*
using System.Collections;
using UnityEngine;

public class MeteorManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] lanes; // 3 lane transformları (x pozisyonları burada)

    [Header("Prefabs / Pool Tags")]
    [SerializeField] private string meteorTag = "Meteor";
    [SerializeField] private string warningTag = "Warning";

    [Header("Spawn Settings")]
    [SerializeField] private float startHeight = 50f;
    [SerializeField] private float initialDelay = 2f;
    [SerializeField] private float minDelay = 0.5f;
    [SerializeField] private float difficultyRate = 0.01f;
    [SerializeField] private float warningDuration = 1f; // Warning gösterim süresi
    [SerializeField] private float spawnHeightOffset = 12f; // kameranın üstünden spawnlama

    private float currentDelay;
    private bool systemActive;

    private void Start()
    {
        currentDelay = initialDelay;
    }

    private void Update()
    {
        if (!systemActive && player.position.y >= startHeight)
        {
            systemActive = true;
            StartCoroutine(SpawnRoutine());
        }

        if (systemActive)
        {
            currentDelay = Mathf.Max(minDelay,
                initialDelay - (player.position.y - startHeight) * difficultyRate);
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (systemActive)
        {
            yield return new WaitForSeconds(currentDelay);

            // Rastgele lane seç
            int laneIndex = Random.Range(0, lanes.Length);
            Vector3 lanePos = lanes[laneIndex].position;

            // Warning spawn
            Vector3 warnPos = new Vector3(lanePos.x, player.position.y + 8f, 0f);
            GameObject warning = ObjectPool._instance.SpawnFromPool(warningTag, warnPos, Quaternion.identity);

            // Meteor spawn zamanını planla + warning referansını gönder
            StartCoroutine(SpawnMeteorAfterDelay(lanePos, warningDuration, warning));
        }
    }

    private IEnumerator SpawnMeteorAfterDelay(Vector3 lanePos, float delay, GameObject warning)
    {
        yield return new WaitForSeconds(delay);

        // Meteor spawn
        Vector3 meteorPos = new Vector3(lanePos.x, player.position.y + spawnHeightOffset, 0f);
        ObjectPool._instance.SpawnFromPool(meteorTag, meteorPos, Quaternion.identity);

        // Meteor çıktı -> warning havuza geri gönder
        if (warning != null)
        {
            ObjectPool._instance.ReturnToPool(warningTag, warning);
        }
    }
}
*/

    // =========== Meteor Manager version 2 ============
/*
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeteorManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera cam;

    [Header("Prefabs / Pool Tags")]
    [SerializeField] private string meteorTag = "Meteor";
    [SerializeField] private string warningTag = "Warning";

    [Header("Spawn Settings")]
    [SerializeField] private float warningDuration = 1.2f;
    [SerializeField] private float meteorSpawnAbovePlatform = 14f; // platformun üstünden spawn
    [SerializeField] private float meteorTriggerDistance = 3f;     // oyuncu bu kadar yaklaştığında meteor düşmeye başlar
    [SerializeField] private float warningOffsetAbovePlayer = 2f;  // oyuncunun üstünde gösterim mesafesi
    [SerializeField] private float meteorStagger = 0.25f;          // aynı row’daki meteorlar arası gecikme

    // Sorted Dictionary: rowY -> list of platform positions
    private SortedDictionary<float, List<Vector3>> pendingMeteors = new SortedDictionary<float, List<Vector3>>();

    void OnEnable()
    {
        PlatformSpawner.OnMeteorRowDetected += RegisterMeteor;
    }

    void OnDisable()
    {
        PlatformSpawner.OnMeteorRowDetected -= RegisterMeteor;
    }

    void Update()
    {
        if (pendingMeteors.Count == 0) return;

        // En alttaki meteor satırını kontrol et
        float firstRowY = pendingMeteors.Keys.Min();
        float distanceToRow = firstRowY - player.position.y;

        // Oyuncu yeterince yaklaştığında meteor spawn
        if (distanceToRow <= meteorTriggerDistance)
        {
            var positions = pendingMeteors[firstRowY];
            pendingMeteors.Remove(firstRowY);

            StartCoroutine(SpawnMeteorRowWithWarnings(positions));
        }
    }

    private void RegisterMeteor(float rowY, Vector3 platformPos)
    {
        if (!pendingMeteors.ContainsKey(rowY))
            pendingMeteors[rowY] = new List<Vector3>();

        pendingMeteors[rowY].Add(platformPos);
    }

    private IEnumerator SpawnMeteorRowWithWarnings(List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            // Warning pozisyonu — oyuncunun üstünde ve lane hizasında
            float camTop = cam.transform.position.y + cam.orthographicSize - 1f;
            float warnY = Mathf.Min(player.position.y + warningOffsetAbovePlayer, camTop);

            Vector3 warnPos = new Vector3(pos.x, warnY, 0f);
            GameObject warning = ObjectPool._instance.SpawnFromPool(warningTag, warnPos, Quaternion.identity);

            // Meteor spawn’ı biraz gecikmeli
            StartCoroutine(SpawnMeteorAfterDelay(pos, warningDuration, warning));

            yield return new WaitForSeconds(meteorStagger);
        }
    }

    private IEnumerator SpawnMeteorAfterDelay(Vector3 platformPos, float delay, GameObject warning)
    {
        yield return new WaitForSeconds(delay);

        // Meteor spawn platformun üstünde
        Vector3 meteorPos = new Vector3(platformPos.x, platformPos.y + meteorSpawnAbovePlatform, 0f);
        ObjectPool._instance.SpawnFromPool(meteorTag, meteorPos, Quaternion.identity);

        // Warning geri gönder
        if (warning != null)
            ObjectPool._instance.ReturnToPool(warningTag, warning);
    }
}


----------------------- Game Manager --------------------------
public static GameManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject startMenuPanel; 
    [SerializeField] private TextMeshProUGUI bestScoreText; 
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject endMenuPanel;
    [SerializeField] private TextMeshProUGUI endBestScoreText; 

    [Header("Player Tracking")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject player;

    public bool isGameActive = false; 
    private float score = 0f;
    private float startY;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 0f; 
        isGameActive = false;

        if (scoreText != null) scoreText.text = "";
        if (startMenuPanel != null) 
        {
            startMenuPanel.SetActive(true);
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            if(bestScoreText != null) bestScoreText.text = " ";
        }
        if (endMenuPanel != null)
        {
            endMenuPanel.SetActive(false);
        }

        if (playerTransform != null) startY = playerTransform.position.y;
    }

    void Update()
    {
        if (!isGameActive || playerTransform == null) return;

        float currentHeight = playerTransform.position.y - startY;
        if (currentHeight > score)
        {
            score = currentHeight;
            UpdateScoreUI();
        }
    }

    public void StartGame()
    {
        isGameActive = true;
        Time.timeScale = 1f; // Zamanı akıt!
        
        if (startMenuPanel != null) 
        {
            startMenuPanel.SetActive(false); 
        }
        if (pauseButton != null)
        {
            pauseButton.SetActive(true);
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = Mathf.FloorToInt(score).ToString();
    }

    public bool IsGameActive()
    {
        return isGameActive;
    }

    public void TriggerGameOver()
    {
        if (!isGameActive) return;
        isGameActive = false;
        Time.timeScale = 0f;

        int currentScoreInt = Mathf.FloorToInt(score);
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (currentScoreInt > highScore)
        {
            highScore = currentScoreInt;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }

        if (startMenuPanel != null)
            startMenuPanel.SetActive(false);

        if (endMenuPanel != null)
            endMenuPanel.SetActive(true);

        if (endBestScoreText != null)
            endBestScoreText.text = "Best " + highScore;
    
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
*/
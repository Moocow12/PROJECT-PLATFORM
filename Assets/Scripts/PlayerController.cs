using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private bool isMovingDown = false;
    [SerializeField] private bool canDoubleJump = false;
    [SerializeField] private bool canMove = false;
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private float acceleration = 1.0f;
    [SerializeField] private float jumpPower = 1.0f;
    [SerializeField] private float doubleJumpMult = 0.5f;
    [SerializeField] private GameObject jumpDust;
    [SerializeField] private Vector3 jumpDustOffset;
    [SerializeField] private GameObject runDustLeft;
    [SerializeField] private GameObject runDustRight;
    [SerializeField] private Vector3 runDustOffset;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip doubleJumpSound;

    private bool isDead = false;
    private Vector2 targetMoveDir = Vector2.zero;
    private Vector2 moveDir = Vector2.zero;
    private Vector2 prevMoveDir = Vector2.zero;
    private Vector2 startPos;
    private Rigidbody2D rbody;
    private GameObject playerRenderer;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    public Rigidbody2D GetRigidbody()
    {
        return rbody;
    }

    public void SetCollisionEnabled(bool state)
    {
        GetComponent<BoxCollider2D>().enabled = state;
    }

    public void SetVisible(bool state)
    {
        spriteRenderer.forceRenderingOff = !state;
    }

    public void SetCanMoveEnabled(bool state)
    {
        canMove = state;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void ResetPlayer()
    {
        transform.position = startPos;
        ResetMoveDir();
        isDead = false;
        SetCollisionEnabled(true);
    }

    public void Win()
    {
        ResetMoveDir();
        moveDir.y = jumpPower;
        isGrounded = false;
    }

    public void Die()
    {
        ResetMoveDir();
        moveDir.y = jumpPower;
        isDead = true;
        SetCollisionEnabled(false);
    }

    public void ResetMoveDir()
    {
        targetMoveDir = Vector2.zero;
        moveDir = Vector2.zero;
    }

    public void Jump(bool isDoubleJump = false)
    {
        if (isDoubleJump)
        {
            moveDir.y = jumpPower * doubleJumpMult;
            audioSource.clip = doubleJumpSound;
            audioSource.Play();
        }
        else
        {
            moveDir.y = jumpPower;
            audioSource.clip = jumpSound;
            audioSource.Play();
        }
        
        Instantiate(jumpDust, transform.position + jumpDustOffset, Quaternion.identity);
    }

    private void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
        playerRenderer = transform.GetChild(0).gameObject;
        animator = playerRenderer.GetComponent<Animator>();
        spriteRenderer = playerRenderer.GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        ProcessInput();
    }

    private void FixedUpdate()
    {
        ProcessPosition();
        ProcessGraphics();

        prevMoveDir = moveDir;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.gameObject.tag == "Ground" || collision.gameObject.tag == "WinPlatform") && isMovingDown)
        {
            isGrounded = true;
            canDoubleJump = false;
            moveDir.y = 0.0f;
            transform.position = new Vector3(transform.position.x, collision.gameObject.transform.position.y + (collision.gameObject.transform.localScale.y * 0.4f));

            if (collision.gameObject.tag == "WinPlatform" && !GameManager.instance.GetGameEnded())
            {
                GameManager.instance.ProcessGameEnd(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = false;
            canDoubleJump = true;
        }
    }

    private void ProcessInput()
    {
        targetMoveDir.x = 0.0f;
        if (!canMove)
        {
            return;
        }

        if (Input.GetKey(KeyCode.A))
        {
            targetMoveDir.x += -1.0f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            targetMoveDir.x += 1.0f;
        }
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || canDoubleJump))
        {
            
            if (canDoubleJump)
            {
                Jump(true);
                canDoubleJump = false;
            }
            else
            {
                Jump();
            }
        }
    }

    private void ProcessPosition()
    {
        if (!rbody.simulated)
        {
            return;
        }

        if (!isGrounded)
        {
            moveDir.y -= GlobalUtility.GRAVITY * Time.deltaTime;
        }
        else
        {
            moveDir.x = Mathf.MoveTowards(moveDir.x, targetMoveDir.x, acceleration * Time.deltaTime);
        }

        rbody.velocity = new Vector2(moveDir.x * moveSpeed, moveDir.y) * Time.deltaTime;
        isMovingDown = (rbody.velocity.y < 0.0f);
    }

    private void ProcessGraphics()
    {
        if (isDead)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
            {
                animator.SetTrigger("Hurt");
            }
        }
        else
        {
            if (isGrounded)
            {
                if (moveDir.x > 0.0f && prevMoveDir.x <= 0.0f)
                {
                    Instantiate(runDustLeft, transform.position + new Vector3(runDustOffset.x, runDustOffset.y), Quaternion.identity);
                }
                else if (moveDir.x < 0.0f && prevMoveDir.x >= 0.0f)
                {
                    Instantiate(runDustRight, transform.position + new Vector3(-runDustOffset.x, runDustOffset.y), Quaternion.identity);
                }

                if (targetMoveDir.x != 0.0f)
                {
                    spriteRenderer.flipX = (targetMoveDir.x < 0.0f);
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                    {
                        animator.SetTrigger("Walk");
                    }
                }
                else
                {
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    {
                        animator.SetTrigger("Idle");
                    }
                }
            }
            else
            {
                if (isMovingDown)
                {
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Fall"))
                    {
                        animator.SetTrigger("Fall");
                    }
                }
                else
                {
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
                    {
                        animator.SetTrigger("Jump");
                    }
                }
            }
        }
    }
}

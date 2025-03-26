using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 move;
    public float forwardSpeed;
    public float maxSpeed;

    private int desiredLane = 1;
    public float laneDistance = 2.5f;

    public bool isGrounded;
    public LayerMask groundLayer;
    public Transform groundCheck;

    public float gravity = -12f;
    public float jumpHeight = 2;
    private Vector3 velocity;

    public Animator animator;
    private bool isSliding = false;

    public float slideDuration = 1.5f;

    bool toggle = false;
    private bool isImmune = false; 

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Time.timeScale = 1.2f;
    }

    private void FixedUpdate()
    {
        if (!PlayerManager.isGameStarted || PlayerManager.gameOver)
            return;

        //Increase Speed
        if (toggle)
        {
            toggle = false;
            if (forwardSpeed < maxSpeed)
                forwardSpeed += 0.1f * Time.fixedDeltaTime;
        }
        else
        {
            toggle = true;
            if (Time.timeScale < 2f)
                Time.timeScale += 0.005f * Time.fixedDeltaTime;
        }
    }

    void Update()
    {
        if (!PlayerManager.isGameStarted || PlayerManager.gameOver)
            return;

        animator.SetBool("isGameStarted", true);
        move.z = forwardSpeed;      // Move forward continuously 

        isGrounded = Physics.CheckSphere(groundCheck.position, 0.17f, groundLayer);
        animator.SetBool("isGrounded", isGrounded);
        if (isGrounded && velocity.y < 0)
            velocity.y = -1f;

        if (isGrounded)
          // touch input from Swipe Manager
        {
            if (SwipeManager.swipeUp)
                Jump();

            if (SwipeManager.swipeDown && !isSliding)
                StartCoroutine(Slide());
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
            if (SwipeManager.swipeDown && !isSliding)
            {
                StartCoroutine(Slide());
                velocity.y = -10;  // Fast fall when sliding  logic
            }                

        }
        controller.Move(velocity * Time.deltaTime);

          
        if (SwipeManager.swipeRight)
        {
            desiredLane++;
            if (desiredLane == 3)
                desiredLane = 2;
        }
        if (SwipeManager.swipeLeft)
        {
            desiredLane--;
            if (desiredLane == -1)
                desiredLane = 0;
        }

        // Move smoothly towards target lane / road 
        Vector3 targetPosition = transform.position.z * transform.forward + transform.position.y * transform.up;
        if (desiredLane == 0)
            targetPosition += Vector3.left * laneDistance;
        else if (desiredLane == 2)
            targetPosition += Vector3.right * laneDistance;

        
        if (transform.position != targetPosition)
        {
            Vector3 diff = targetPosition - transform.position;
            Vector3 moveDir = diff.normalized * 30 * Time.deltaTime;
            if (moveDir.sqrMagnitude < diff.magnitude)
                controller.Move(moveDir);
            else
                controller.Move(diff);
        }

        controller.Move(move * Time.deltaTime);
    }

    private void Jump()
    {   Debug.Log("inside jump");
        StopCoroutine(Slide());
        animator.SetBool("isSliding", false);
        animator.SetTrigger("jump");
        controller.center = Vector3.zero;
        controller.height = 2;
        isSliding = false;
   
        velocity.y = Mathf.Sqrt(jumpHeight * 2 * -gravity);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {    // collision logic 
         if (isImmune) return; 
        if (isImmune) return;  

    if (hit.transform.CompareTag("Obstacle"))
    {     Debug.Log(" detect obstacles");
        FindObjectOfType<AudioManager>().PlaySound("GameOver");
        ReduceSpeed();
        FindObjectOfType<PlayerManager>().TakeDamage(30);
        StartCoroutine(DamageCooldown()); // Start immunity
        StartCoroutine(DestroyObstacle(hit.gameObject));
         FindObjectOfType<WheelAI>().AttackPlayer();
         FindObjectOfType<TileManager>().SpawnEnemy(transform.position);
    }
    else if (hit.transform.CompareTag("Enemy"))
    {    Debug.Log("Detect Enemy");
        FindObjectOfType<PlayerManager>().TakeDamage(45);
        StartCoroutine(DamageCooldown()); // Start immunity will take
    }
    }
    private IEnumerator DamageCooldown()
{    // damge after some time
    isImmune = true;
    yield return new WaitForSeconds(2f);  
    isImmune = false;
}      
private IEnumerator DestroyObstacle(GameObject obstacle)
{    // destroy colllider of obstacles 
      yield return new WaitForSeconds(1.2f);  
    if (obstacle != null)  
    {
        Collider col = obstacle.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;  
        }
    }
}
    public void ReduceSpeed()
{
    forwardSpeed *= 0.5f; 
    move.z = forwardSpeed; 
    StartCoroutine(RestoreSpeed());
}
private IEnumerator RestoreSpeed()
{      Debug.Log("inside restore health");
    yield return new WaitForSeconds(0.45f); 

    while (forwardSpeed < maxSpeed)
    {
        forwardSpeed += 0.1f; 
        move.z = forwardSpeed; 
        yield return new WaitForSeconds(0.7f);
    }

    forwardSpeed = maxSpeed; 
    move.z = forwardSpeed; 
}


    private IEnumerator Slide()
    {      
        isSliding = true;
        animator.SetBool("isSliding", true);
        yield return new WaitForSeconds(0.25f/ Time.timeScale);
        controller.center = new Vector3(0, -0.5f, 0);
        controller.height = 1;
         Debug.Log("Sliding");

        yield return new WaitForSeconds((slideDuration - 0.25f)/Time.timeScale);

        animator.SetBool("isSliding", false);

        controller.center = Vector3.zero;
        controller.height = 2;

        isSliding = false;
    }
}

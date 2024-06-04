using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Security.Cryptography;
using Unity.VisualScripting;

public class EnemyAI : MonoBehaviour
{
    [Header("Pathfinding")]
    public Transform mainTarget;
    public float activationDistance;
    public float pathUpdateSeconds;


    [Header("Physics")]
    public float speed = 200f;
    public float animationSpeed;
    public float wanderSpeed;
    public float nextWaypointDistance = 3f;
    public float jumpNodeHeightRequirement = 0.8f;
    public float jumpModifier = 0.3f;
    public float jumpCheckOffset = 0.1f;
    public int jumpChance;
    public LayerMask groundMask;

    [Header("Costum Behavior")]
    public bool followEnabled = true;
    public bool jumpEnabled = true;
    public bool directionLookEnabled = true;
    public float freezeTime;
    public float freezeMovementPenalty;
    public bool frozen;

    
    [SerializeField] private float colorChangeTime = 0.5f;
    [SerializeField] private float freezed;
    [SerializeField] private int wanderDirection;
    [SerializeField] private bool directionRight;
    [SerializeField] private Animator animator;
    private SpriteRenderer sr;
    private Path path;
    private int currentWaypoint = 0;
    bool isGrounded = false;
    Seeker seeker;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Transform groundCheck;
    [SerializeField] Vector2 groundBox;

    private void Start()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator.SetBool("isGrounded", true);

        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
        wanderDirection = 1;
        freezed = 1;
    }

    private void FixedUpdate()
    {
        if (directionLookEnabled)
        {
            if (rb.velocity.x > 1)
            {
                gameObject.GetComponent<SpriteRenderer>().flipX = true;
            }
            else if (rb.velocity.x < 1)
            {
                gameObject.GetComponent<SpriteRenderer>().flipX = false;
            }
        }

        animator.speed = Mathf.Abs(rb.velocity.x * animationSpeed);
        animator.SetFloat("wolfVelocity", rb.velocity.x);
        if (rb.velocity.x > 0)
        {
            directionRight = true;
        }
        else if (rb.velocity.x < 0)
        {
            directionRight = false;
        }

        if (TargetInDistance() && followEnabled)
        {
            PathFollow();
        }
        else
        {
            Wander();
        }

        animator.SetBool("isGrounded", isGrounded);
    }

    private void Wander()
    {
        if (rb.velocity.x == 0 && directionRight == true)
        {
            wanderDirection = 1;
        }
        else if (rb.velocity.x == 0 && directionRight == false)
        {
            wanderDirection = -1;
        }
        rb.AddForce(Vector2.left * wanderSpeed * wanderDirection);
        if (TargetInDistance() && followEnabled)
        {
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WanderBarrier"))
        {
            wanderDirection = wanderDirection * -1;
        }

        if (collision.gameObject.layer == 10 && !frozen)
        {
            StartCoroutine(Freeze());
            frozen = true;
            
        }
    }

    private IEnumerator Freeze()
    {
        freezed = freezeMovementPenalty;
        float timeElapsed = 0;
        Color tmp1 = sr.color;

        while (timeElapsed < colorChangeTime)
        {
            timeElapsed += Time.deltaTime;

            tmp1.r = Mathf.Lerp(1f, 0, timeElapsed / colorChangeTime);
            tmp1.g = Mathf.Lerp(1f, 0.75f, timeElapsed / colorChangeTime);

            sr.color = tmp1;
            yield return null;
        }

        sr.color = new Color(0, 0.75f, 1f, 1f);
        yield return new WaitForSeconds(freezeTime);
        timeElapsed = 0;

        while (timeElapsed < colorChangeTime)
        {
            timeElapsed += Time.deltaTime;

            tmp1.r = Mathf.Lerp(0, 1f, timeElapsed / colorChangeTime);
            tmp1.g = Mathf.Lerp(0.75f, 1f, timeElapsed / colorChangeTime);

            sr.color = tmp1;
            yield return null;
        }

        sr.color = Color.white;
        
        freezed = 1;
        frozen = false;
    }

    private void UpdatePath()
    {
        if (TargetInDistance() && followEnabled)
        {
            seeker.StartPath(rb.position, mainTarget.position, OnPathComplete);
        }
    }

    private void PathFollow()
    {
        
        if (path == null)
        {
            return;
        }
        
        if (currentWaypoint >= path.vectorPath.Count)
        {
            return;
        }
        
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundBox, groundMask.value);
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = new Vector2(direction[0], 0.0f) * speed * Time.deltaTime * freezed;

        if (jumpEnabled && isGrounded)
        {
            jumpChance = Random.Range(1, 4);
            if (direction.y > jumpNodeHeightRequirement && jumpChance == 1)
            {
                rb.AddForce(Vector2.up * speed * jumpModifier);
            }
        }

        if (isGrounded)
        {
            rb.AddForce(force);
        }
        


        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

    }

    private bool TargetInDistance()
    {
        return Vector2.Distance(transform.position, mainTarget.transform.position) < activationDistance;
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)    
        {
            path = p;
            currentWaypoint = 0;
        }
    }
}

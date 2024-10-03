using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Security.Cryptography;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList;

public class EnemyAI : MonoBehaviour
{
    [Header("Pathfinding")]
    private Transform mainTarget;
    public float activationDistance;
    public float pathUpdateSeconds;


    [Header("Physics")]
    public float speed = 200f;
    public float animationSpeed;
    public float wanderSpeed;
    public float nextWaypointDistance = 3f;
    public float jumpNodeHeightRequirement = 0.8f;
    public float jumpModifier = 0.3f;
    public int jumpChanceMax;
    public LayerMask groundMask;

    [Header("Costum Behavior")]
    public bool followEnabled = true;
    public bool wanderEnabled = true;
    public bool jumpEnabled = true;
    public bool directionLookEnabled = true;
    public float freezeTime;
    public float freezeMovementPenalty;
    public bool frozen;


    public int damage = 20;
    [SerializeField] private float deathFadeTime = 5f;    
    [SerializeField] private float colorChangeTime = 0.5f;
    [SerializeField] private float freezed;
    [SerializeField] private int wanderDirection;
    [SerializeField] private bool directionRight;
    [SerializeField] private Animator animator;
    private int jumpChance;
    private SpriteRenderer sr;
    private Path path;
    private int currentWaypoint = 0;
    public bool isGrounded = false;
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

        mainTarget = GameObject.Find("PlayerGroundCheck").transform;

        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
        wanderDirection = 1;
        freezed = 1;
    }

    private void FixedUpdate()
    {
        if (GetComponent<Health>().dead)
        {
            followEnabled = false;
            wanderEnabled = false;
            jumpEnabled = false;
            directionLookEnabled = false;
            animationSpeed = 0;
            gameObject.layer = 7;
            StopCoroutine(Freeze());
            StartCoroutine(DeathFade());
        }

        if (directionLookEnabled)
        {
            if (rb.velocity.x > 1)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            else if (rb.velocity.x < 1)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
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
        else if (wanderEnabled)
        {
            Wander();
        }

        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundBox, 0, groundMask.value);
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

        sr.color = new Color(0, 0.75f, sr.color.g, sr.color.a);
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

        sr.color = new Color(1f, 1f, sr.color.g, sr.color.a);
        
        freezed = 1;
        frozen = false;
    }

    private IEnumerator DeathFade() 
    {
        sr.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        float timeElapsed = 0;
        Color tmp1 = sr.color;

        while (timeElapsed < deathFadeTime) 
        {
            timeElapsed += Time.deltaTime;

            tmp1.a = Mathf.Lerp(1f, 0f, timeElapsed / deathFadeTime);

            sr.color = tmp1;
            yield return null;
        }

        sr.color = new Color(0.2f, 0.2f, 0.2f, 0f);

        Destroy(gameObject);
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
        
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = new Vector2(direction[0], 0.0f) * speed * Time.deltaTime * freezed;

        if (jumpEnabled && isGrounded)
        {
            jumpChance = Random.Range(1, jumpChanceMax);
            if (direction.y > jumpNodeHeightRequirement && jumpChance == 1)
            {
                rb.AddForce(Vector2.up * jumpModifier);
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

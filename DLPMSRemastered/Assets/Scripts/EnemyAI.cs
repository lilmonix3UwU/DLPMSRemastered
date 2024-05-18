using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Security.Cryptography;

public class EnemyAI : MonoBehaviour
{
    [Header("Pathfinding")]
    public Transform target;
    public float activationDistance;
    public float pathUpdateSeconds;


    [Header("Physics")]
    public float speed = 200f;
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

    private Path path;
    private int currentWaypoint = 0;
    bool isGrounded = false;
    Seeker seeker;
    [SerializeField] Rigidbody2D rb;

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
    }

    private void FixedUpdate()
    {
        if (TargetInDistance() && followEnabled)
        {
            PathFollow();
        }
    }

    private void UpdatePath()
    {
        if (TargetInDistance() && followEnabled)
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
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
        
        isGrounded = Physics2D.Raycast(transform.position, -Vector3.up, GetComponent<Collider2D>().bounds.extents.y + jumpCheckOffset, groundMask);
       
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = new Vector2(direction[0], 0.0f) * speed * Time.deltaTime;

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

        if (directionLookEnabled)
        {
            if (rb.velocity.x > 0.05f)
            {
                transform.GetComponent<SpriteRenderer>().flipX = true;
            }
            else if (rb.velocity.x > -0.05f)
            {
                transform.GetComponent<SpriteRenderer>().flipX = false;
            }
        }
    }

    private bool TargetInDistance()
    {
        return Vector2.Distance(transform.position, target.transform.position) < activationDistance;
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

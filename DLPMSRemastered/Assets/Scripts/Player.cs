using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb; 
    [SerializeField] private Animator anim;
    [SerializeField] private Transform graphic;
    [SerializeField] private RuntimeAnimatorController animCtrl; 
    [SerializeField] private RuntimeAnimatorController torchAnimCtrl;
    
    private InputManager _input;
    private AudioManager _audio;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float accel = 0.2f;
    [SerializeField] private float accelMult = 5f;
    [SerializeField] private float decel = 0.1f;
    [SerializeField] private float slowDownSpeed = 2f; 
    [SerializeField] private float revertTime = 0.3f;

    [SerializeField] private float _curSpeed;
    private float _move;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 3f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius;
    [SerializeField] private LayerMask groundMask;
    
    private bool _grounded;
    private float _coyouteTimeCounter;
    
    [Header("Attacking")]
    [SerializeField] private float attackCooldown = 5f;

    private float cooldownTimer = Mathf.Infinity;

    [Header("Effects")]
    [SerializeField] private ParticleSystem impactEffect;
    [SerializeField] private ParticleSystem landEffect;
    [SerializeField] private ParticleSystem jumpEffect;

    private void Start()
    {
        _input = InputManager.Instance;
        _audio = AudioManager.Instance;
    }

    private void FixedUpdate()
    {
        // Move
        rb.velocity = new Vector2(_move * _curSpeed, rb.velocity.y);
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
            return;
        
        // Get Move Input
        _move = _input.Horizontal();
        
        // Acceleration & Deceleration
        if (_move != 0f && _curSpeed < maxSpeed)
        {
            _curSpeed += accel;
        }
        if (_move == 0f && _curSpeed > 0)
        {
            _curSpeed -= decel;
        }

        // Jump
        _grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask.value);
        
        if (_input.PressJump() && _coyouteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpEffect.Play();
        }
        if (_input.ReleaseJump() && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);

            _coyouteTimeCounter = 0f;
        }

        if (rb.velocity.y < 0f)
        {
            rb.velocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime);
        }

        if (!_grounded && rb.velocity.y > -2f && rb.velocity.y < 2f && _input.HoldJump())
        {
            _curSpeed += accel * accelMult;
        }
        else
        {
            _curSpeed = _curSpeed > maxSpeed ? maxSpeed : _curSpeed;
            _curSpeed = _curSpeed < 0f ? 0f : _curSpeed;
        }

        // Animation
        if (_move == 0f)
        {
            anim.SetBool("Running", false);
        }
        else if (_move < 0f)
        {
            graphic.rotation = Quaternion.Euler(0f, 0f, 0f);
            anim.SetBool("Running", true);
        }
        else if (_move > 0f)
        {
            graphic.rotation = Quaternion.Euler(0f, 180f, 0f);
            anim.SetBool("Running", true);
        }

        if (!_grounded)
        {
            _coyouteTimeCounter -= Time.deltaTime;
            
            anim.SetFloat("TimeInAir", anim.GetFloat("TimeInAir") + Time.deltaTime);
            
            anim.SetBool("InAir", true);
        }
        else
        {
            _coyouteTimeCounter = coyoteTime;
            
            anim.SetBool("InAir", false);
            Invoke(nameof(ResetTimeInAir), 0.15f);
        }

        
        // Equipping
        if (_input.PressEquip())
        {
            anim.runtimeAnimatorController = anim.runtimeAnimatorController == animCtrl ? torchAnimCtrl : animCtrl;

            if (anim.runtimeAnimatorController == torchAnimCtrl)
            {
                _audio.Play("Ignite");
                _audio.Play("Torch Burning");
            }
            else
            {
                _audio.Play("Blow");
                _audio.Stop("Torch Burning");
            }
        }
        
        // Attacking
        if (_input.PressAttack() && cooldownTimer > attackCooldown)
        {
            anim.SetTrigger("Attack");
            cooldownTimer = 0f;
        }
        else
        {
            cooldownTimer += Time.deltaTime;   
        }
    }

    private void Footstep()
    {   
        _audio.Play(RandomFootstepSound());
        impactEffect.Play();
    }

    private void LandEffect()
    {
        _audio.Play(RandomLandSound());
        landEffect.Play();
    }

    private void LandSound()
    {
        _audio.Play(RandomLandSound());
    }

    private void SlowDown()
    {
        StartCoroutine(SlowDownCoroutine(slowDownSpeed, revertTime));
    }

    private IEnumerator SlowDownCoroutine(float speed, float revertTime)
    {
        _curSpeed = speed;

        float timeElapsed = 0f;
        while (timeElapsed < revertTime)
        {
            _curSpeed = Mathf.Lerp(_curSpeed, maxSpeed, timeElapsed / revertTime);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        _curSpeed = maxSpeed;
        yield return null;
    }

    private void ResetTimeInAir()
    {
        anim.SetFloat("TimeInAir", 0f);
    }

    private string RandomFootstepSound()
    {
        string footstepSound = "";

        switch (Random.Range(0, 3))
        {
            case 0:
                footstepSound = "Footstep1";
                break;
            case 1:
                footstepSound = "Footstep2";
                break;
            case 2:
                footstepSound = "Footstep3";
                break;
        }

        return footstepSound;
    }

    private string RandomLandSound()
    {
        string landSound = "";

        switch (Random.Range(0, 3))
        {
            case 0:
                landSound = "Land1";
                break;
            case 1:
                landSound = "Land2";
                break;
            case 2:
                landSound = "Land3";
                break;
        }

        return landSound;
    }
}

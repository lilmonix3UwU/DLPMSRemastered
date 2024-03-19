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

    private float _curSpeed;
    private float _xMove;
    private float _yMove;
    private int _facingDir;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 3f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.4f;
    [SerializeField] private LayerMask groundLayer;

    private bool _grounded;
    private float _timeInAir;
    private float _coyouteTimeCounter;

    [Header("Wall Sliding/Jumping")]
    [SerializeField] private float wallSlideSpeed = 4f;
    [SerializeField] private float wallJumpDuration = 0.4f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(8f, 16f);
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallRadius = 0.4f;
    [SerializeField] private LayerMask wallLayer;

    private float _wallJumpTime;
    private float _wallJumpCounter;
    private int _wallJumpDir;
    private bool _walled;
    private bool _wallSliding;
    private bool _wallJumping;

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

    private void Update()
    {
        if (Time.timeScale == 0f)
            return;

        HandleMoveInput();
        HandleJumping();
        HandleWallJump();
        HandleWallSlide();
        HandleSpriteFlip();
        HandleAnimation();
        HandleEquipping();
        HandleAttacking();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (_wallJumping)
            return;

        rb.velocity = new Vector2(_xMove * _curSpeed, rb.velocity.y);
    }

    private void HandleMoveInput()
    {
        _xMove = _input.Move().x;

        // Acceleration & Deceleration
        if ((_xMove != 0f || _yMove != 0f) && _curSpeed < maxSpeed)
        {
            _curSpeed += accel;
        }
        if ((_xMove == 0f || _yMove == 0f) && _curSpeed > 0)
        {
            _curSpeed -= decel;
        }
    }

    private void HandleJumping()
    {
        _grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer.value);

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

        // Slight boost forward in air when holding jump
        if (!_grounded && !_walled && rb.velocity.y > -2f && rb.velocity.y < 2f && _input.HoldJump())
        {
            _curSpeed += accel * accelMult;
        }
        else
        {
            _curSpeed = _curSpeed > maxSpeed ? maxSpeed : _curSpeed;
            _curSpeed = _curSpeed < 0f ? 0f : _curSpeed;
        }

        // Coyote Time
        if (!_grounded)
        {
            _coyouteTimeCounter -= Time.deltaTime;

            _timeInAir += Time.deltaTime;
        }
        else
        {
            _coyouteTimeCounter = coyoteTime;

            Invoke(nameof(ResetTimeInAir), 0.15f);
        }
    }

    private void HandleWallSlide()
    {
        _walled = Physics2D.OverlapCircle(wallCheck.position, wallRadius, wallLayer.value);

        if (_walled && !_grounded && _xMove != 0f)
        {
            _wallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue));
        }
        else
        {
            _wallSliding = false;
        }
    }

    private void HandleWallJump()
    {
        if (_wallSliding)
        {
            _wallJumping = false;
            _wallJumpDir = -_facingDir;
            _wallJumpCounter = _wallJumpTime;

            CancelInvoke(nameof(StopWallJump));
        }
        else
        {
            _wallJumpTime -= Time.deltaTime;
        }

        if (_input.PressJump() && _walled)
        {
            _wallJumping = true;
            rb.velocity = _wallJumpDir * wallJumpForce;
            _wallJumpCounter = 0f;

            if (_facingDir != _wallJumpDir)
            {
                graphic.rotation = _facingDir == -1 ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.Euler(0f, 0f, 0f);
            }

            Invoke(nameof(StopWallJump), wallJumpDuration);
        }
    }

    private void StopWallJump()
    {
        _wallJumping = false;
    }

    private void HandleSpriteFlip()
    {
        if (_wallJumping)
            return;

        if (_xMove < 0f)
        {
            graphic.rotation = Quaternion.Euler(0f, 0f, 0f);
            _facingDir = -1;
        }
        else if (_xMove > 0f)
        {
            graphic.rotation = Quaternion.Euler(0f, 180f, 0f);
            _facingDir = 1;
        }
    }

    private void HandleAnimation()
    {
        anim.SetFloat("xMove", _xMove);
        anim.SetBool("Grounded", _grounded);
        anim.SetBool("Attack", _input.PressEquip());
        anim.SetFloat("TimeInAir", _timeInAir);
        anim.SetBool("WallSliding", _wallSliding);
    }

    private void HandleEquipping()
    {
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
    }

    private void HandleAttacking()
    {
        if (_input.PressAttack() && cooldownTimer > attackCooldown)
        {
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
        _timeInAir = 0;
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
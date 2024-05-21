using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb; 
    [SerializeField] private Animator anim;
    [SerializeField] private Transform graphic;
    [SerializeField] private Health health;
    [SerializeField] private RuntimeAnimatorController animCtrl; 
    [SerializeField] private RuntimeAnimatorController torchAnimCtrl;
    
    private InputManager _input;
    private AudioManager _audio;
    private GameOverManager _gameOver;

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
    private float _coyoteTimeCounter;

    [Header("Wall Sliding/Jumping")]
    [SerializeField] private float wallSlideSpeed = 4f;
    [SerializeField] private float wallJumpDuration = 0.4f;
    [SerializeField] private float _wallJumpTime = 0.2f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(8f, 16f);
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallRadius = 0.4f;
    [SerializeField] private LayerMask wallLayer;


    private float _wallJumpCounter;
    private int _wallJumpDir;
    private bool _walled;
    private bool _wallSliding;
    private bool _wallJumping;

    [Header("Attacking")]
    [SerializeField] private Transform fireballSpawn;
    [SerializeField] private float attackCooldown = 5f;
    [SerializeField] private float fireballSpeed = 50f;

    private float _cooldownTimer = Mathf.Infinity;
    private bool _attacking;

    [Header("Effects")]
    [SerializeField] private ParticleSystem impactEffect;
    [SerializeField] private ParticleSystem landEffect;
    [SerializeField] private ParticleSystem jumpEffect;
    [SerializeField] private ParticleSystem wallJumpEffect;
    [SerializeField] private GameObject fireball;
    [SerializeField] private GameObject fireEffectNormal;
    [SerializeField] private GameObject fireEffectGreen;
    [SerializeField] private GameObject fireEffectBlue;

    [Header("UI")]
    [SerializeField] private Animator torchAnim;

    private void Start()
    {
        _input = InputManager.Instance;
        _audio = AudioManager.Instance;
        _gameOver = GameOverManager.Instance;
    }

    private void Update()
    {
        if (health.dead)
        {
            _xMove = 0;
            _grounded = true;
            _cooldownTimer = 0;
            _timeInAir = 0;
            _wallSliding = false;
            HandleAnimation();

            _gameOver.GameOver();

            return;
        }

        if (Time.timeScale == 0f || _attacking)
            return;

        HandleMoveInput();
        HandleJumping();
        HandleWallJump();
        HandleWallSlide();
        HandleSpriteFlip();
        HandleAnimation();
        HandleAttacking();
        HandleSwitching();
    }

    private void FixedUpdate()
    {
        if (_attacking || health.dead)
            return;
        
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

        if (_input.PressJump() && _coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpEffect.Play();
        }
        if (_input.ReleaseJump() && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);

            _coyoteTimeCounter = 0f;
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
            _coyoteTimeCounter -= Time.deltaTime;

            _timeInAir += Time.deltaTime;
        }
        else
        {
            _coyoteTimeCounter = coyoteTime;

            Invoke(nameof(ResetTimeInAir), 0.15f);
        }
    }

    private void HandleWallSlide()
    {
        _walled = Physics2D.OverlapCircle(wallCheck.position, wallRadius, wallLayer.value);

        if (_walled && !_grounded && _xMove == _facingDir)
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
        if (!_wallSliding)
        {
            _wallJumpCounter -= Time.deltaTime;
        }
        else
        {
            _wallJumping = false;
            _wallJumpDir = -_facingDir;
            _wallJumpCounter = _wallJumpTime;

            CancelInvoke(nameof(StopWallJump));
        }

        if (_input.PressJump() && (_wallSliding || _wallJumpCounter > 0f))
        {
            wallJumpEffect.Play();
            
            _wallJumping = true;
            rb.velocity = new Vector2(_wallJumpDir * wallJumpForce.x, wallJumpForce.y);
            _wallJumpCounter = 0f;

            if (_facingDir != _wallJumpDir)
            {
                graphic.rotation = _facingDir == -1 ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.Euler(0f, 0f, 0f);
                _facingDir = -_facingDir;
            }

            Invoke(nameof(StopWallJump), wallJumpDuration);
        }
        if (_input.ReleaseJump() && rb.velocity.y > 0f)
        {
            _wallJumpCounter = 0f;
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
        anim.SetBool("Attack", _input.PressAttack() && _cooldownTimer > attackCooldown);
        anim.SetFloat("TimeInAir", _timeInAir);
        anim.SetBool("WallSliding", _wallSliding);
    }

    private void HandleSwitching()
    {
        if (_input.PressSlot1())
        {
            torchAnim.SetInteger("Torch", 1);

            fireEffectNormal.SetActive(true);
            fireEffectBlue.SetActive(false);
            fireEffectGreen.SetActive(false);
        }
        if (_input.PressSlot2())
        {
            torchAnim.SetInteger("Torch", 2);

            fireEffectNormal.SetActive(false);
            fireEffectBlue.SetActive(true);
            fireEffectGreen.SetActive(false);
        }
        if (_input.PressSlot3())
        {
            torchAnim.SetInteger("Torch", 3);

            fireEffectNormal.SetActive(false);
            fireEffectBlue.SetActive(false);
            fireEffectGreen.SetActive(true);
        }
    }

    /*private void HandleEquipping()
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
    }*/

    private void HandleAttacking()
    {
        if (_input.PressAttack() && _cooldownTimer > attackCooldown)
        {
            _cooldownTimer = 0f;

            StartCoroutine(ShootFireball());
        }
        else
        {
            _cooldownTimer += Time.deltaTime;
        }
    }

    private IEnumerator ShootFireball()
    {
        _attacking = true;
        
        rb.velocity = Vector3.zero;
        rb.gravityScale = 0;
        
        // Spawn fireball
        GameObject fireballGO = Instantiate(fireball, fireballSpawn.position, Quaternion.identity);

        // Get facing direction
        Vector3 facingDir = -graphic.right;

        yield return new WaitForSeconds(0.5f);

        rb.gravityScale = 4;
        
        _attacking = false;
        
        // Play sound
        _audio.Play("Ignite");

        // Move fireball
        float timeElapsed = 0f;

        while (timeElapsed < 3f)
        {
            fireballGO.gameObject.transform.position += facingDir * fireballSpeed * Time.deltaTime;

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        Destroy(fireballGO);
        yield return null;
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
        StartCoroutine(SlowDownCoroutine());
    }

    private IEnumerator SlowDownCoroutine()
    {
        _curSpeed = slowDownSpeed;

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
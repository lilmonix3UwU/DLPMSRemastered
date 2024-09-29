using System.Collections;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb; 
    [SerializeField] private Animator anim;
    [SerializeField] private RuntimeAnimatorController plrTorchAnim;
    [SerializeField] private RuntimeAnimatorController plrAnim;
    [SerializeField] private Transform graphic;
    [SerializeField] private Health health;
    [SerializeField] private CameraFollowObject camFollowObj;

    private InputManager _inputMgr;
    private AudioManager _audioMgr;
    private GameOverManager _gameOverMgr;
    private CameraManager _camMgr;

    [HideInInspector] public bool onlyAnimate;
    [HideInInspector] public bool gettingPushed;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float accel = 0.2f;
    [SerializeField] private float decel = 0.1f;
    [SerializeField] private float slowDownSpeed = 2f; 
    [SerializeField] private float revertTime = 0.3f;

    private float _curSpeed;
    private float _move;

    [HideInInspector] public int facingDir;

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

    private float _wallJumpCounter;
    private int _wallJumpDir;
    private bool _walled;
    private bool _wallSliding;
    private bool _wallJumping;

    [Header("Attacking")]
    [SerializeField] private GameObject fireball;
    [SerializeField] private GameObject ice;
    [SerializeField] private Transform fireballSpawn;
    [SerializeField] private Transform iceSpawn;
    [SerializeField] private float attackCooldown = 5f;
    [SerializeField] private float fireballSpeed = 50f;
    [SerializeField] private float minDistForFocus = 10f;

    private float _cooldownTimer = Mathf.Infinity;
    private bool _attacking;

    private int _fireTorchAmount = 0;
    private int _iceTorchAmount = 0;
    private int _uniqueTorches = 0;
    
    [HideInInspector] public int curTorch = 0;

    private bool _firstFireTorch = true;
    private bool _firstIceTorch = true;

    [Header("Effects")]
    [SerializeField] private ParticleSystem impactEffect;
    [SerializeField] private ParticleSystem landEffect;
    [SerializeField] private ParticleSystem jumpEffect;
    [SerializeField] private ParticleSystem wallJumpEffect;
    [SerializeField] private GameObject fireEffect;
    [SerializeField] private GameObject iceEffect;

    [Header("UI")]
    [SerializeField] private Animator torchAnim;
    [SerializeField] private TMP_Text fireTorchText;
    [SerializeField] private TMP_Text iceTorchText;
    [SerializeField] private GameObject fireTorchUI;
    [SerializeField] private GameObject iceTorchUI;

    private void Start()
    {
        _inputMgr = InputManager.Instance;
        _audioMgr = AudioManager.Instance;
        _gameOverMgr = GameOverManager.Instance;
        _camMgr = CameraManager.Instance;

        fireTorchText.text = _fireTorchAmount.ToString();
        iceTorchText.text = _iceTorchAmount.ToString();

        fireTorchUI.SetActive(false);
        iceTorchUI.SetActive(false);

        HandleTorchEffects();
    }

    private void Update()
    {
        if (health.dead)
        {
            _move = 0;
            _grounded = true;
            _cooldownTimer = 0;
            _timeInAir = 0;
            _wallSliding = false;
            HandleAnimation();

            _gameOverMgr.GameOver();

            return;
        }

        if (Time.timeScale == 0)
        {
            anim.speed = 0;
            return;
        }
        else 
            anim.speed = 1;

        if (_attacking)
            return;

        HandleMoveInput();
        HandleJumping();
        HandleWallJump();
        HandleWallSlide();
        HandleAnimation();
        HandleAttacking();
        HandleSwitching();
        HandleCameraEffects();
    }

    private void FixedUpdate()
    {
        if (_attacking || health.dead || Time.timeScale == 0)
            return;
        
        HandleMovement();
        HandleSpriteFlip();
    }

    private void HandleMovement()
    {
        if (_wallJumping || gettingPushed)
            return;

        rb.velocity = new Vector2(_move * _curSpeed, rb.velocity.y);
    }

    private void HandleMoveInput()
    {
        if (onlyAnimate)
        {
            _move = 0;
            return;
        }

        _move = _inputMgr.Move();

        // Acceleration & Deceleration
        if (_move != 0f && _curSpeed < maxSpeed)
            _curSpeed += accel * Time.deltaTime;
        if (_move == 0f && _curSpeed > 0)
            _curSpeed -= decel * Time.deltaTime;
    }

    private void HandleJumping()
    {
        _grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer.value);

        if (rb.velocity.y < 0f)
            rb.velocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime);

        if (onlyAnimate)
            return;

        if (_inputMgr.PressJump() && _coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpEffect.Play();
            _audioMgr.Play("Jump");
        }
        if (_inputMgr.ReleaseJump() && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);

            _coyoteTimeCounter = 0f;
        }

        // Slight boost forward in air when holding jump
        if (!_grounded && !_walled && rb.velocity.y > -2f && rb.velocity.y < 2f && _inputMgr.HoldJump())
            _curSpeed += accel * Time.deltaTime;
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
        _walled = Physics2D.OverlapCircle(wallCheck.position, wallRadius, groundLayer.value);

        if (_walled && !_grounded && _move == facingDir)
        {
            _wallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue));
        }
        else
            _wallSliding = false;
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
            _wallJumpDir = -facingDir;
            _wallJumpCounter = _wallJumpTime;

            CancelInvoke(nameof(StopWallJump));
        }

        if (_inputMgr.PressJump() && (_wallSliding || _wallJumpCounter > 0f))
        {
            wallJumpEffect.Play();
            _audioMgr.Play("Jump");

            _wallJumping = true;
            rb.velocity = new Vector2(_wallJumpDir * wallJumpForce.x, wallJumpForce.y);
            _wallJumpCounter = 0f;

            if (facingDir != _wallJumpDir)
            {
                graphic.rotation = facingDir == -1 ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.Euler(0f, 0f, 0f);
                facingDir = -facingDir;
                camFollowObj.CallTurn(facingDir == -1 ? 180f : 0f);
            }

            Invoke(nameof(StopWallJump), wallJumpDuration);
        }
        if (_inputMgr.ReleaseJump() && rb.velocity.y > 0f)
            _wallJumpCounter = 0f;
    }

    private void StopWallJump() => _wallJumping = false;

    private void HandleSpriteFlip()
    {
        if (_wallJumping)
            return;

        if (_move < 0f && facingDir != -1) // Left
        {
            graphic.rotation = Quaternion.Euler(0f, 0f, 0f);
            facingDir = -1;
            camFollowObj.CallTurn(0f);
        }
        else if (_move > 0f && facingDir != 1) // Right
        {
            graphic.rotation = Quaternion.Euler(0f, 180f, 0f);
            facingDir = 1;
            camFollowObj.CallTurn(180f);
        }
    }

    private void HandleAnimation()
    {
        // Player
        anim.SetFloat("xMove", _move);
        anim.SetBool("Grounded", _grounded);
        anim.SetFloat("TimeInAir", _timeInAir);
        anim.SetBool("WallSliding", _wallSliding);
        if (anim.runtimeAnimatorController == plrTorchAnim) 
            anim.SetBool("Attack", _inputMgr.PressAttack() && _cooldownTimer > attackCooldown && _grounded);
    }

    private void HandleSwitching()
    {
        if (_inputMgr.PressSlot1() && _fireTorchAmount > 0)
        {
            anim.runtimeAnimatorController = plrTorchAnim;
            curTorch = 1;

            fireEffect.SetActive(true);
            iceEffect.SetActive(false);
        }
        if (_inputMgr.PressSlot2() && _iceTorchAmount > 0)
        {
            anim.runtimeAnimatorController = plrTorchAnim;
            curTorch = 2;

            fireEffect.SetActive(false);
            iceEffect.SetActive(true);
        }

        torchAnim.SetInteger("CurrentTorch", curTorch);
    }

    private void HandleAttacking()
    {
        if (onlyAnimate || !_grounded)
            return;

        if (_inputMgr.PressAttack() && _cooldownTimer > attackCooldown)
        {
            switch (curTorch)
            {
                case 1:
                    if (_fireTorchAmount <= 0) return;
                    _cooldownTimer = 0f;

                    StartCoroutine(ShootFireball());
                    break;
                case 2:
                    if (_iceTorchAmount <= 0) return;
                    _cooldownTimer = 0f;

                    StartCoroutine(ShootIce());
                    break;
            }
        }
        else
            _cooldownTimer += Time.deltaTime;
    }

    private void HandleCameraEffects() 
    {
        if (rb.velocity.y < _camMgr.fallSpeedYDampingChangeThreshold && !_camMgr.LerpingYDamping && !_camMgr.LerpedFromPlayerFalling)
            _camMgr.LerpYDamping(true);

        if (rb.velocity.y >= 0f && !_camMgr.LerpingYDamping && _camMgr.LerpedFromPlayerFalling) 
        {
            _camMgr.LerpedFromPlayerFalling = false;

            _camMgr.LerpYDamping(false);
        }
    }

    private IEnumerator ShootFireball()
    {
        _attacking = true;
        
        rb.velocity = Vector3.zero;
        
        // Spawn fireball
        GameObject fireballGO = Instantiate(fireball, fireballSpawn.position, Quaternion.identity);

        // Play sounds
        _audioMgr.Play("Shoot");

        yield return new WaitForSeconds(0.5f);

        int curFacingDir = facingDir;

        _fireTorchAmount--;
        fireTorchText.text = _fireTorchAmount.ToString();

        HandleTorchEffects();

        _attacking = false;

        // Move fireball
        float timeElapsed = 0f;

        while (timeElapsed < 3f)
        {
            fireballGO.gameObject.transform.position += new Vector3(curFacingDir, 0, 0) * fireballSpeed * Time.deltaTime;

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        Destroy(fireballGO);
        yield return null;
    }

    private IEnumerator ShootIce()
    {
        _attacking = true;

        rb.velocity = Vector3.zero;

        yield return new WaitForSeconds(0.5f);

        // Spawn ice
        GameObject iceGO = Instantiate(ice, iceSpawn.position, graphic.rotation);

        _iceTorchAmount--;
        iceTorchText.text = _iceTorchAmount.ToString();

        HandleTorchEffects();

        _attacking = false;

        // Play sounds
        _audioMgr.Play("Ice");
        yield return new WaitForSeconds(0.1f);
        _audioMgr.Play("Ice");

        yield return new WaitForSeconds(1.9f);

        Destroy(iceGO);
        yield return null;
    }

    private void Footstep()
    {   
        _audioMgr.Play(RandomFootstepSound());
        impactEffect.Play();
    }

    private void LandEffect()
    {
        _audioMgr.Play(RandomLandSound());
        landEffect.Play();
    }

    private void LandSound() => _audioMgr.Play(RandomLandSound());

    private void SlowDown() => StartCoroutine(SlowDownCoroutine());

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

    private void ResetTimeInAir() => _timeInAir = 0;

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

    private void HandleTorchEffects()
    {
        if (_fireTorchAmount <= 0 && _iceTorchAmount <= 0)
        {
            anim.runtimeAnimatorController = plrAnim;

            fireEffect.SetActive(false);
            iceEffect.SetActive(false);
            curTorch = 0;
        }
        else if (_fireTorchAmount <= 0 && _iceTorchAmount > 0 && curTorch != 2)
        {
            fireEffect.SetActive(false);
            iceEffect.SetActive(true);
            curTorch = 2;
        }
        else if (_fireTorchAmount > 0 && _iceTorchAmount <= 0 && curTorch != 1)
        {
            fireEffect.SetActive(true);
            iceEffect.SetActive(false);
            curTorch = 1;
        }

        torchAnim.SetInteger("CurrentTorch", curTorch);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "FireTorch")
        {
            if (_fireTorchAmount <= 0 && _iceTorchAmount <= 0 && curTorch != 1)
            {
                anim.runtimeAnimatorController = plrTorchAnim;
                fireEffect.SetActive(true);
                curTorch = 1;
                torchAnim.SetInteger("CurrentTorch", curTorch);

                if (_firstFireTorch) 
                    StartCoroutine(ShowFireTorchUI());
            }

            _fireTorchAmount++;
            fireTorchText.text = _fireTorchAmount.ToString();

            _audioMgr.Play("PickUp");
            Destroy(collision.gameObject);
        }
        if (collision.tag == "IceTorch")
        {
            if (_fireTorchAmount <= 0 && _iceTorchAmount <= 0 && curTorch != 2)
            {
                anim.runtimeAnimatorController = plrTorchAnim;
                iceEffect.SetActive(true);
                curTorch = 2;
                torchAnim.SetInteger("CurrentTorch", curTorch);

                if (_firstIceTorch) 
                    StartCoroutine(ShowIceTorchUI());
            }
            else if (_fireTorchAmount > 0 && _iceTorchAmount <= 0 && _firstIceTorch) 
            {
                fireEffect.SetActive(false);
                iceEffect.SetActive(true);

                curTorch = 2;
                torchAnim.SetInteger("CurrentTorch", curTorch);

                StartCoroutine(ShowIceTorchUI());
            }

            _iceTorchAmount++;
            iceTorchText.text = _iceTorchAmount.ToString();

            _audioMgr.Play("PickUp");
            Destroy(collision.gameObject);
        }
    }

    private IEnumerator ShowFireTorchUI() 
    {
        _firstFireTorch = false;
        onlyAnimate = true;

        fireTorchUI.SetActive(true);
        _audioMgr.Play("Ignite");

        yield return new WaitForSeconds(0.5f);

        _audioMgr.Play("PickUpMusic");
        
        yield return new WaitForSeconds(6f);

        _uniqueTorches++;
        torchAnim.SetInteger("UniqueTorches", _uniqueTorches);

        Destroy(fireTorchUI);
        onlyAnimate = false;

    }

    private IEnumerator ShowIceTorchUI() 
    {
        _firstIceTorch = false;
        onlyAnimate = true;

        iceTorchUI.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        _audioMgr.Play("PickUpMusic");

        yield return new WaitForSeconds(6f);

        _uniqueTorches++;
        torchAnim.SetInteger("UniqueTorches", _uniqueTorches);

        Destroy(iceTorchUI);
        onlyAnimate = false;
    }
}
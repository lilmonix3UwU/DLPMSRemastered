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
    
    private InputManager _input;
    private AudioManager _audio;
    private GameOverManager _gameOver;

    [HideInInspector] public bool onlyAnimate;
    [HideInInspector] public bool gettingPushed;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float accel = 0.2f;
    [SerializeField] private float accelMult = 5f;
    [SerializeField] private float decel = 0.1f;
    [SerializeField] private float slowDownSpeed = 2f; 
    [SerializeField] private float revertTime = 0.3f;

    private float _curSpeed;
    private float _move;
    private int _facingDir;

    [HideInInspector] public bool faceEnemy;

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
    private int _curTorch = 0;

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
        _input = InputManager.Instance;
        _audio = AudioManager.Instance;
        _gameOver = GameOverManager.Instance;

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

            _gameOver.GameOver();

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

        _move = _input.Move();

        // Acceleration & Deceleration
        if (_move != 0f && _curSpeed < maxSpeed)
            _curSpeed += accel;
        if (_move == 0f && _curSpeed > 0)
            _curSpeed -= decel;
    }

    private void HandleJumping()
    {
        _grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer.value);

        if (rb.velocity.y < 0f)
            rb.velocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime);

        if (onlyAnimate)
            return;

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

        // Slight boost forward in air when holding jump
        if (!_grounded && !_walled && rb.velocity.y > -2f && rb.velocity.y < 2f && _input.HoldJump())
            _curSpeed += accel * accelMult;
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

        if (_walled && !_grounded && _move == _facingDir)
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
            _wallJumpCounter = 0f;
    }

    private void StopWallJump() => _wallJumping = false;

    private void HandleSpriteFlip()
    {
        if (_wallJumping)
            return;
        
        bool closeEnoughToEnemy;

        if (ClosestEnemy() != null)
            closeEnoughToEnemy = Vector2.Distance(transform.position, ClosestEnemy().transform.position) < minDistForFocus;
        else
            closeEnoughToEnemy = false;

        if (_input.PressFocus() && closeEnoughToEnemy)
            faceEnemy = !faceEnemy;

        if (!closeEnoughToEnemy && faceEnemy) 
            faceEnemy = false;
        else if (closeEnoughToEnemy && !faceEnemy)
            faceEnemy = true;

        if (faceEnemy) 
        {
            Vector2 dir = (ClosestEnemy().transform.position - transform.position).normalized;
            
            if (dir.x < 0f) 
            {
                graphic.rotation = Quaternion.Euler(0f, 0f, 0f);
                _facingDir = -1;
            }
            else if (dir.x > 0f) 
            {
                graphic.rotation = Quaternion.Euler(0f, 180f, 0f);
                _facingDir = 1;
            }

            return;
        }

        if (_move < 0f) // Left
        {
            graphic.rotation = Quaternion.Euler(0f, 0f, 0f);
            _facingDir = -1;
        }
        else if (_move > 0f) // Right
        {
            graphic.rotation = Quaternion.Euler(0f, 180f, 0f);
            _facingDir = 1;
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
            anim.SetBool("Attack", _input.PressAttack() && _cooldownTimer > attackCooldown && _grounded);
    }

    private void HandleSwitching()
    {
        if (_input.PressSlot1() && _fireTorchAmount > 0)
        {
            anim.runtimeAnimatorController = plrTorchAnim;
            _curTorch = 1;

            fireEffect.SetActive(true);
            iceEffect.SetActive(false);
        }
        if (_input.PressSlot2() && _iceTorchAmount > 0)
        {
            anim.runtimeAnimatorController = plrTorchAnim;
            _curTorch = 2;

            fireEffect.SetActive(false);
            iceEffect.SetActive(true);
        }

        torchAnim.SetInteger("CurrentTorch", _curTorch);
    }

    private void HandleAttacking()
    {
        if (onlyAnimate || !_grounded)
            return;

        if (_input.PressAttack() && _cooldownTimer > attackCooldown)
        {
            switch (_curTorch)
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

    private IEnumerator ShootFireball()
    {
        _attacking = true;
        
        rb.velocity = Vector3.zero;
        
        // Spawn fireball
        GameObject fireballGO = Instantiate(fireball, fireballSpawn.position, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);

        int curFacingDir = _facingDir;

        _fireTorchAmount--;
        fireTorchText.text = _fireTorchAmount.ToString();

        HandleTorchEffects();

        _attacking = false;
        
        // Play sounds
        _audio.Play("Ignite");

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
        _audio.Play("Ice");
        yield return new WaitForSeconds(0.1f);
        _audio.Play("Ice");

        yield return new WaitForSeconds(1.9f);

        Destroy(iceGO);
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

    private void LandSound() => _audio.Play(RandomLandSound());

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
            _curTorch = 0;

            _audio.Stop("Torch Burning");
        }
        else if (_fireTorchAmount <= 0 && _iceTorchAmount > 0 && _curTorch != 2)
        {
            fireEffect.SetActive(false);
            iceEffect.SetActive(true);
            _curTorch = 2;

            _audio.Stop("Torch Burning");
        }
        else if (_fireTorchAmount > 0 && _iceTorchAmount <= 0 && _curTorch != 1)
        {
            fireEffect.SetActive(true);
            iceEffect.SetActive(false);
            _curTorch = 1;

            _audio.Play("Torch Burning");
        }

        torchAnim.SetInteger("CurrentTorch", _curTorch);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "FireTorch")
        {
            if (_fireTorchAmount <= 0 && _iceTorchAmount <= 0 && _curTorch != 1)
            {
                anim.runtimeAnimatorController = plrTorchAnim;
                fireEffect.SetActive(true);
                _curTorch = 1;
                torchAnim.SetInteger("CurrentTorch", _curTorch);

                if (_firstFireTorch)
                    StartCoroutine(ShowFireTorchUI());
            }

            _audio.Play("Torch Burning");

            _fireTorchAmount++;
            fireTorchText.text = _fireTorchAmount.ToString();

            _audio.Play("Pick Up");
            Destroy(collision.gameObject);
        }
        if (collision.tag == "IceTorch")
        {
            if (_fireTorchAmount <= 0 && _iceTorchAmount <= 0 && _curTorch != 2)
            {
                anim.runtimeAnimatorController = plrTorchAnim;
                iceEffect.SetActive(true);
                _curTorch = 2;
                torchAnim.SetInteger("CurrentTorch", _curTorch);

                if (_firstIceTorch)
                    StartCoroutine(ShowIceTorchUI());
            }
            else if (_fireTorchAmount > 0 && _iceTorchAmount <= 0 && _firstIceTorch) 
            {
                fireEffect.SetActive(false);
                iceEffect.SetActive(true);

                _curTorch = 2;
                torchAnim.SetInteger("CurrentTorch", _curTorch);

                StartCoroutine(ShowIceTorchUI());
            }

            _iceTorchAmount++;
            iceTorchText.text = _iceTorchAmount.ToString();

            _audio.Play("Pick Up");
            Destroy(collision.gameObject);
        }
    }

    public GameObject ClosestEnemy() 
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closestEnemy = null;
        float closestDist = Mathf.Infinity;

        foreach (GameObject enemy in enemies) 
        {
            float distToEnemy = Vector2.Distance(transform.position, enemy.transform.position);

            if (distToEnemy < closestDist) 
            {
                closestDist = distToEnemy;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    private IEnumerator ShowFireTorchUI() 
    {
        _firstFireTorch = false;
        onlyAnimate = true;

        fireTorchUI.SetActive(true);
        
        yield return new WaitForSeconds(6.5f);

        _uniqueTorches++;
        torchAnim.SetInteger("UniqueTorches", _uniqueTorches);

        Destroy(fireTorchUI);
        onlyAnimate = false;
        _input.enabled = true;

    }

    private IEnumerator ShowIceTorchUI() 
    {
        _input.enabled = true;
        _firstFireTorch = false;
        onlyAnimate = true;

        iceTorchUI.SetActive(true);

        yield return new WaitForSeconds(6.25f);

        _uniqueTorches++;
        torchAnim.SetInteger("UniqueTorches", _uniqueTorches);

        yield return new WaitForSeconds(0.25f);

        Destroy(iceTorchUI);
        onlyAnimate = false;
    }
}
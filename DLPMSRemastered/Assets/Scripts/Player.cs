using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private RuntimeAnimatorController defaultController;
    [SerializeField] private RuntimeAnimatorController torchController;

    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float slowDownSpeed = 2f;
    [SerializeField] private float timeToRevertToNormalSpeed = 2f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 3f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius;
    [SerializeField] private LayerMask groundMask;

    private float _initialSpeed;
    private bool _grounded;

    private InputManager _input;

    private void Start()
    {
        _input = InputManager.Instance;

        _initialSpeed = speed;
    }

    private void Update()
    {
        // Move
        Vector3 moveDir = new Vector3(_input.Horizontal(), 0, 0);

        transform.position += moveDir * speed * Time.deltaTime;

        // Jump
        _grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask.value);

        if (_input.PressJump() && _grounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // Animation
        if (_input.Horizontal() == 0)
        {
            animator.SetBool("Running", false);
        }
        else if (_input.Horizontal() < 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            animator.SetBool("Running", true);
        }
        else if (_input.Horizontal() > 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            animator.SetBool("Running", true);
        }

        if (!_grounded)
        {
            animator.SetBool("InAir", true);
        }
        else
        {
            animator.SetBool("InAir", false);
        }

        if (_input.PressEquip())
        {
            animator.runtimeAnimatorController = (animator.runtimeAnimatorController == defaultController) ? torchController : defaultController;
        }
    }

    public void SlowDown()
    {
        StartCoroutine(SlowDownCoroutine(slowDownSpeed, timeToRevertToNormalSpeed));
    }

    private IEnumerator SlowDownCoroutine(float speed, float revertTime)
    {
        this.speed = speed;

        float timeElapsed = 0;
        while (timeElapsed < revertTime)
        {
            this.speed = Mathf.Lerp(speed, _initialSpeed, timeElapsed / revertTime);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        this.speed = _initialSpeed;
        yield return null;
    } 
}

using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _jumpForce = 3f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundRadius;
    [SerializeField] private LayerMask _groundMask;

    private bool _grounded;

    private InputManager _input;

    private void Start()
    {
        _input = InputManager.Instance;
    }

    private void Update()
    {
        // Move
        Vector3 moveDir = new Vector3(_input.Horizontal(), 0, 0);

        transform.position += moveDir * _speed * Time.deltaTime;

        // Jump
        _grounded = Physics2D.OverlapCircle(_groundCheck.position, _groundRadius, _groundMask.value);

        if (_input.PressJump() && _grounded)
        {
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }
    }
}

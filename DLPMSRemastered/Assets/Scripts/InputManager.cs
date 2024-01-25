using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;
    public static InputManager Instance { get { return _instance; } }

    private InputMaster _inputMaster;

    private void Awake()
    {
        _instance = this;

        _inputMaster = new InputMaster();
    }

    private void OnEnable() => _inputMaster.Enable();

    private void OnDisable() => _inputMaster.Disable();

    public float Horizontal() => _inputMaster.Player.Move.ReadValue<float>();

    public bool PressJump() => _inputMaster.Player.Jump.WasPressedThisFrame();
}

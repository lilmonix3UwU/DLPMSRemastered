using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    private InputMaster _inputMaster;

    private void Awake()
    {
        Instance = this;

        _inputMaster = new InputMaster();
    }

    private void OnEnable() => _inputMaster.Enable();

    private void OnDisable() => _inputMaster.Disable();

    public float Horizontal() => _inputMaster.Player.Move.ReadValue<float>();

    public bool PressJump() => _inputMaster.Player.Jump.WasPressedThisFrame();

    public bool ReleaseJump() => _inputMaster.Player.Jump.WasReleasedThisFrame();

    public bool HoldJump() => _inputMaster.Player.Jump.IsPressed();

    public bool PressEquip() => _inputMaster.Player.Equip.WasPressedThisFrame();

    public bool PressAttack() => _inputMaster.Player.Attack.WasPressedThisFrame();

    public bool PressPause() => _inputMaster.Player.PauseGame.WasPressedThisFrame();
}

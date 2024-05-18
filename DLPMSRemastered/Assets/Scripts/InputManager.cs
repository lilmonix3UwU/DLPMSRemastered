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

    public Vector2 Move() => _inputMaster.Player.Move.ReadValue<Vector2>();

    public bool PressJump() => _inputMaster.Player.Jump.WasPressedThisFrame();

    public bool ReleaseJump() => _inputMaster.Player.Jump.WasReleasedThisFrame();

    public bool HoldJump() => _inputMaster.Player.Jump.IsPressed();

    public bool PressAttack() => _inputMaster.Player.Attack.WasPressedThisFrame();

    public bool PressPause() => _inputMaster.Player.PauseGame.WasPressedThisFrame();

    public bool PressInteract() => _inputMaster.Player.Interact.WasPressedThisFrame();

    public bool PressSlot1() => _inputMaster.Player.Slot1.WasPressedThisFrame();

    public bool PressSlot2() => _inputMaster.Player.Slot2.WasPressedThisFrame();

    public bool PressSlot3() => _inputMaster.Player.Slot3.WasPressedThisFrame();
}

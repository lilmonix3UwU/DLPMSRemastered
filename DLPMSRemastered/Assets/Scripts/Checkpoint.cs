using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Player plr;
    [SerializeField] private GameObject fire;

    private InputManager _inputMgr;
    private AudioManager _audioMgr;
    private GameManager _gameMgr;

    private void Start() 
    {
        _inputMgr = InputManager.Instance;
        _audioMgr = AudioManager.Instance;
        _gameMgr = GameManager.Instance;

        if (_gameMgr.lastCheckpointPos != (Vector2)transform.position)
            fire.SetActive(false);
        else
            fire.SetActive(true);
    }

    private void OnTriggerStay2D(Collider2D other) 
    {
        if (other.CompareTag("Player") && _inputMgr.PressInteract())
        {
            fire.SetActive(true);

            _audioMgr.Play("Ignite");

            _gameMgr.lastCheckpointPos = transform.position;
            _gameMgr.fireTorchAmount = plr.fireTorchAmount;
            _gameMgr.iceTorchAmount = plr.iceTorchAmount;
            _gameMgr.skipIntro = true;
        }    
    }
}

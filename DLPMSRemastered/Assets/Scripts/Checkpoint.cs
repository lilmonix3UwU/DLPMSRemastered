using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Player plr;
    [SerializeField] private GameObject fire;
    [SerializeField] private Animator pressIndicator;
    [SerializeField] private Vector2 pressIndicatorOffset;

    private InputManager _inputMgr;
    private AudioManager _audioMgr;
    private GameManager _gameMgr;

    [HideInInspector] public bool firstCheckpoint = true;

    private void Start() 
    {
        _inputMgr = InputManager.Instance;
        _audioMgr = AudioManager.Instance;
        _gameMgr = GameManager.Instance;

        firstCheckpoint = _gameMgr.firstCheckpoint;

        if (firstCheckpoint)
            pressIndicator.transform.position = (Vector2)transform.position + pressIndicatorOffset;

        if (_gameMgr.lastCheckpointPos != (Vector2)transform.position)
            fire.SetActive(false);
        else
            fire.SetActive(true);
    }

    private void OnTriggerStay2D(Collider2D other) 
    {
        if (other.CompareTag("Player") && _inputMgr.PressInteract())
        {
            if (firstCheckpoint)
                pressIndicator.SetTrigger("Pressed");

            fire.SetActive(true);

            _audioMgr.Play("Ignite");

            _gameMgr.lastCheckpointPos = transform.position;
            _gameMgr.fireTorchAmount = plr.fireTorchAmount;
            _gameMgr.iceTorchAmount = plr.iceTorchAmount;
            _gameMgr.curTorch = plr.curTorch;
            _gameMgr.uniqueTorches = plr.uniqueTorches;
            _gameMgr.skipIntro = true;
            _gameMgr.firstCheckpoint = false;
        }    
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (firstCheckpoint && collision.CompareTag("Player"))
            pressIndicator.SetBool("Active", true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (firstCheckpoint && collision.CompareTag("Player"))
            pressIndicator.SetBool("Active", false);
    }
}

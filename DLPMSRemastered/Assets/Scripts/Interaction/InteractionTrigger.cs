using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform plr;
    [SerializeField] private Transform interactionBox;

    private InteractionManager _interaction;
    private InputManager _input;

    [Header("Configuration")] 
    [SerializeField] private Sentence[] sentences;
    [SerializeField] private float minDist;
    [SerializeField] private Vector2 boxOffset;

    private bool _interacting;

    private void Start()
    {
        _interaction = InteractionManager.Instance;
        _input = InputManager.Instance;
    }

    private void Update()
    {
        interactionBox.transform.position = (Vector2)transform.position + boxOffset;

        if (Vector2.Distance(transform.position, plr.position) < minDist && (!_interacting || _input.PressInteract()))
        {
            _interacting = true;
            _interaction.Interact(sentences);
        }
        else if (Vector2.Distance(transform.position, plr.position) > minDist && _interaction.interactionActive)
        {
            _interaction.EndInteraction();
            _interacting = false;
        }
    }
}
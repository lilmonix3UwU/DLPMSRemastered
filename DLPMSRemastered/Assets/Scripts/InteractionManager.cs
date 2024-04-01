using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class InteractionManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private Animator interactionBoxAnimator;
    
    private Queue<Sentence> _sentences;
    private Sentence _curSentence;
    
    private bool _typing; 
    [HideInInspector] public bool interactionActive;

    [Header("Configuration")]
    [SerializeField] private float nextTimeToInteract = 1f;
    
    private float _interactTimer;

    private IEnumerator _typingCoroutine;
    
    public static InteractionManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _sentences = new Queue<Sentence>();
    }

    private void Update()
    {
        if (!interactionActive)
        {
            _interactTimer += Time.deltaTime;
        }
    }

    public void Interact(Sentence[] interaction)
    {
        if (!interactionActive && _interactTimer >= nextTimeToInteract)
        {
           StartCoroutine(StartInteraction(interaction));
        }
        else if (interactionActive)
        {
            NextSentence();
        }
    }

    private IEnumerator StartInteraction(Sentence[] interaction)
    {
        text.text = ""; // Clear the text
        
        interactionBoxAnimator.SetBool("IsOpen", true);

        yield return new WaitForSeconds(1f);
        
        interactionActive = true;
        _interactTimer = 0f;

        _sentences.Clear();

        foreach (Sentence sentence in interaction)
        {
            _sentences.Enqueue(sentence);
        }

        NextSentence();
    }

    private void NextSentence()
    {
        if (_sentences.Count == 0 && !_typing)
        {
            EndInteraction();
        }
        else if (_typing)
        {
            SkipTyping();
        }
        else if (!_typing)
        {
            _curSentence = _sentences.Dequeue();

            _typingCoroutine = TypeSentence(_curSentence);
            StartCoroutine(_typingCoroutine);
        }
    }

    private IEnumerator TypeSentence(Sentence sentence)
    {
        _typing = true;

        text.text = ""; // Clear the text
        foreach (char letter in sentence.text)
        {
            text.text += letter;

            if (letter == '!' || letter == ',' || letter == '.' || letter == '?') 
                yield return new WaitForSeconds(sentence.secondsPerWord * 8);
            else
                yield return new WaitForSeconds(sentence.secondsPerWord);
        }

        _typing = false;
    }

    private void SkipTyping()
    {
        StopCoroutine(_typingCoroutine);

        text.text = _curSentence.text;
        _typing = false;
    }

    public void EndInteraction()
    {
        StopCoroutine(_typingCoroutine);

        interactionBoxAnimator.SetBool("IsOpen", false);

        interactionActive = false;
    }
}

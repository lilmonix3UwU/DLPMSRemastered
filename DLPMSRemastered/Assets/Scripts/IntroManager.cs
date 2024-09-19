using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private Player plr;
    [SerializeField] private Rigidbody2D plrRb;
    [SerializeField] private Transform grandma;
    [SerializeField] private Animator grandmaAnim;
    [SerializeField] private CanvasGroup introUI;
    [SerializeField] private GameObject interactionBox;
    [SerializeField] private VideoClip cutscene;
    [SerializeField] private GameObject chapterUI;
    [SerializeField] private CameraFollow camScript;

    [SerializeField] private Transform startPos;
    [SerializeField] private Transform endPos;
    [SerializeField] private float grandmaWalkTime = 3f;

    private bool _doneWalking;
    private bool _hasSkipped;

    private AudioManager _audio;
    private InputManager _input;
    private InteractionManager _interaction;

    private void Start()
    {
        chapterUI.SetActive(false);
        introUI.gameObject.SetActive(true);

        plr.enabled = false;
        plrRb.gravityScale = 0;

        _input = InputManager.Instance;
        _interaction = InteractionManager.Instance;

        _audio = AudioManager.Instance;
        _audio.Play("Music");
        _audio.Play("Ambience");

        camScript.enabled = false;

        StartCutscene();
    }

    private void Update()
    {
        if (_input.SkipCutscene() && !_hasSkipped)
        {
            _input.enabled = false;
            StopAllCoroutines();
            _hasSkipped = true;
            StartCutscene();
        }

        if (!_interaction.interactionActive && _doneWalking)
        {
            StartCoroutine(GrandmaWalk(endPos.position, startPos.position));
        }
    }

    private void StartCutscene()
    {
        StartCoroutine(FadeIntro());
        StartCoroutine(GrandmaWalk(startPos.position, endPos.position));
    }

    private IEnumerator FadeIntro()
    {
        if (!_hasSkipped)
            yield return new WaitForSeconds((float)cutscene.length);

        plr.enabled = true;
        plrRb.gravityScale = 4;

        float elapsedTime = 0;

        while (elapsedTime < 1)
        {
            elapsedTime += Time.deltaTime;
            introUI.alpha = Mathf.Lerp(1, 0, elapsedTime);
            yield return null;
        }

        introUI.alpha = 0;
        introUI.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        camScript.enabled = true;
    }

    private IEnumerator GrandmaWalk(Vector3 from, Vector3 to)
    {
        if (!_hasSkipped)
            yield return new WaitForSeconds((float)cutscene.length + 0.5f);
        else
            yield return new WaitForSeconds(0.5f);

        _doneWalking = false;

        plr.onlyAnimate = true;

        grandmaAnim.SetBool("Walking", true);
        grandma.rotation = from.x - to.x < 0 ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);

        float elapsedTime = 0;

        while (elapsedTime < grandmaWalkTime)
        {
            elapsedTime += Time.deltaTime;
            grandma.position = Vector3.Lerp(from, to, elapsedTime / grandmaWalkTime);
            yield return null;
        }

        grandma.position = to;

        grandmaAnim.SetBool("Walking", false);

        if (grandma.position != startPos.position) 
        {
            _doneWalking = true;
            _input.enabled = true;
        }
        else
        {
            grandma.gameObject.SetActive(false);
            interactionBox.SetActive(false);
            plr.onlyAnimate = false;

            chapterUI.SetActive(true);

            yield return new WaitForSeconds(4);

            chapterUI.SetActive(false);
        }
    }
}

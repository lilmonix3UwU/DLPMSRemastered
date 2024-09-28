using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private Player plr;
    [SerializeField] private Rigidbody2D plrRb;
    [SerializeField] private CanvasGroup introUI;
    [SerializeField] private VideoClip cutscene;
    [SerializeField] private GameObject chapterUI;
    [SerializeField] private CameraFollow camScript;

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
            StopCoroutine(nameof(StartCutscene));

            _hasSkipped = true;
            StartCutscene();
        }
    }

    private void StartCutscene()
    {
        StartCoroutine(FadeIntro());
        StartCoroutine(ShowChapterUI());
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

    private IEnumerator ShowChapterUI()
    {
        if (!_hasSkipped)
            yield return new WaitForSeconds((float)cutscene.length + 2);
        else
            yield return new WaitForSeconds(2);

        chapterUI.SetActive(true);

        yield return new WaitForSeconds(4);

        plr.onlyAnimate = false;
        _input.enabled = true;

        Destroy(chapterUI);
        Destroy(introUI.gameObject);
        Destroy(gameObject);
    }
}

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

    private bool _doneWalking;
    private bool _hasSkipped;

    private InputManager _inputMgr;
    private AudioManager _audioMgr;
    private GameManager _gameMgr;

    private void Start()
    {
        _inputMgr = InputManager.Instance;
        _audioMgr = AudioManager.Instance;
        _gameMgr = GameManager.Instance;

        if (_gameMgr.skipIntro)
        {
            Destroy(gameObject);
            Destroy(introUI.gameObject);
            Destroy(chapterUI);
            return;
        }

        chapterUI.SetActive(false);
        introUI.gameObject.SetActive(true);

        plr.enabled = false;
        plrRb.gravityScale = 0;

        StartCutscene();
    }

    private void Update()
    {
        if (_inputMgr.SkipCutscene() && !_hasSkipped)
        {
            _inputMgr.enabled = false;
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
    }

    private IEnumerator ShowChapterUI()
    {
        if (!_hasSkipped)
            yield return new WaitForSeconds((float)cutscene.length + 2);
        else
            yield return new WaitForSeconds(2);

        chapterUI.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        _audioMgr.Play("ChapterMusic");

        yield return new WaitForSeconds(3.5f);

        plr.onlyAnimate = false;
        _inputMgr.enabled = true;

        Destroy(chapterUI);
        Destroy(introUI.gameObject);
        Destroy(gameObject);
    }
}

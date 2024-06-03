using System.Collections;
using TMPro;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup gameOverMenu;
    [SerializeField] private ParticleSystem textEeffect;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private TMP_Text retryText;

    [SerializeField] private float fadeInTime = 1f;

    [HideInInspector] public bool gameIsOver;

    private bool _fading = false;
    private bool _canDissolve = false;

    public static GameOverManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameOverMenu.gameObject.SetActive(false);
    }

    public void GameOver()
    {
        if (!_fading)
        {
            StartCoroutine(FadeIn());
        }
        if (_canDissolve)
        {
            StartCoroutine(TextDissolve());
        }
    }

    private IEnumerator FadeIn()
    {
        _fading = true;

        yield return new WaitForSeconds(3f);

        gameOverMenu.gameObject.SetActive(true);
        retryText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);

        _canDissolve = true;

        float timeElapsed = 0f;

        while (timeElapsed < fadeInTime)
        {
            timeElapsed += Time.deltaTime;
            gameOverMenu.alpha = Mathf.Lerp(0f, 1f, timeElapsed / fadeInTime);
            yield return null;
        }

        gameOverMenu.alpha = 1f;
    }

    private IEnumerator TextDissolve()
    {
        _canDissolve = false;

        textEeffect.PlayBackward(this);

        yield return new WaitForSeconds(textEeffect.main.duration - 0.03f);

        retryText.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(true);

        retryText.alpha = 0f;

        float timeElapsed = 0f;
        float textAppearTime = 1f;

        while (timeElapsed < textAppearTime)
        {
            timeElapsed += Time.deltaTime;
            retryText.alpha = Mathf.Lerp(0f, 1f, timeElapsed / textAppearTime);
            yield return null;
        }

        retryText.alpha = 1f;
    }
}
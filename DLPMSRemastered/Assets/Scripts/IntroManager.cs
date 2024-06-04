using System.Collections;
using UnityEngine;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private Player plr;
    [SerializeField] private Rigidbody2D plrRb;
    [SerializeField] private CanvasGroup introUI;

    private AudioManager _audio;
    private InputManager _input;

    private void Start()
    {
        introUI.gameObject.SetActive(true);

        plr.enabled = false;
        plrRb.gravityScale = 0;

        _input = InputManager.Instance;
        _input.enabled = false;

        _audio = AudioManager.Instance;
        _audio.Play("Music");

        Invoke("HideIntro", 22.5f);
    }

    private void HideIntro()
    {
        StartCoroutine(FadeIntro());
    }

    private IEnumerator FadeIntro()
    {
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

        yield return new WaitForSeconds(5);

        _input.enabled = true;
    }
}

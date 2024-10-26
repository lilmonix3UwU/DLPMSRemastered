using System.Collections;
using UnityEngine;
using TMPro;

public class TextDissolve : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private TMP_Text retryText;
    

    private void Start()
    {
        StartCoroutine(PlayParticles());
    }

    private IEnumerator PlayParticles()
    {
        retryText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        ps.PlayBackward(this);

        yield return new WaitForSeconds(ps.main.duration - 0.03f);

        retryText.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(true);

        retryText.alpha = 0f;

        float timeElapsed = 0;
        float textAppearTime = 1;

        while (timeElapsed < textAppearTime)
        {
            timeElapsed += Time.deltaTime;
            retryText.alpha = Mathf.Lerp(0f, 1f, timeElapsed / textAppearTime);
            yield return null;
        }

        retryText.alpha = 1f;
    }
}

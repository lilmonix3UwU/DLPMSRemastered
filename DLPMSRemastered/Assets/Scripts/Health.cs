using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Health : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Animator anim;

    [Header("Configuration")]
    [SerializeField] private int maxHealth;
    [SerializeField] private float iFramesAmount = 3f;
    [SerializeField] private float colorChangeTime = 1f;

    private int _curHealth;
    private float _iFrames = 0f;
    private bool _colorChanging = false;

    [HideInInspector] public bool dead = false;
    private bool _initialColorChanging = false;
    private bool _initialHasChanged = false;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;

    [Header("UI (Optional)")]
    [SerializeField] private Image healthSlider;
    [SerializeField] private TMP_Text dmgText;

    private void Start()
    {
        _curHealth = maxHealth;
        UpdateUI();
    }

    private void Update()
    {
        if (_curHealth <= 0)
        {
            dead = true;
            anim.SetTrigger("Die");
        }

        if (_iFrames > 0)
        {
            _iFrames -= Time.deltaTime;
        }
        if (_iFrames > 0 && !_colorChanging && !_initialColorChanging)
        {
            if (!_initialHasChanged)
            {
                StartCoroutine(InitialColorChange());
            }
            else if (_initialHasChanged && !dead)
            {
                StartCoroutine(ColorChange());
            }
        }
        if (_iFrames <= 0)
        {
            StopCoroutine(ColorChange());
            sr.color = Color.white;

            _initialHasChanged = false;
        }
    }

    private IEnumerator InitialColorChange()
    {
        _initialColorChanging = true;

        float timeElapsed = 0;
        Color tmp1 = sr.color;

        while (timeElapsed < colorChangeTime)
        {
            timeElapsed += Time.deltaTime;

            tmp1.a = Mathf.Lerp(0.25f, 1f, timeElapsed / colorChangeTime);
            tmp1.g = Mathf.Lerp(0f, 1f, timeElapsed / colorChangeTime);
            tmp1.b = Mathf.Lerp(0f, 1f, timeElapsed / colorChangeTime);

            sr.color = tmp1;
            yield return null;
        }

        sr.color = Color.white;

        _initialColorChanging = false;
        _initialHasChanged = true;
    }

    private IEnumerator ColorChange()
    {
        _colorChanging = true;

        float timeElapsed = 0;
        Color tmp1 = sr.color;

        while (timeElapsed < colorChangeTime)
        {
            timeElapsed += Time.deltaTime;

            tmp1.a = Mathf.Lerp(1f, 0.25f, timeElapsed / colorChangeTime);

            sr.color = tmp1;
            yield return null;
        }

        sr.color = new Color(1f, 1f, 1f, 0.25f);

        timeElapsed = 0;

        while (timeElapsed < colorChangeTime)
        {
            timeElapsed += Time.deltaTime;

            tmp1.a = Mathf.Lerp(0.25f, 1f, timeElapsed / colorChangeTime);

            sr.color = tmp1;
            yield return null;
        }

        sr.color = Color.white;

        _colorChanging = false;
    }

    //Collision med enemies
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (CompareTag("Player") && collision.gameObject.CompareTag("Enemy") && _iFrames <= 0)
        {
            int dmg = 20;

            dmgText.text = dmg.ToString();

            _curHealth -= dmg;
            UpdateUI();

            _iFrames = iFramesAmount;

            // Hit effect
            GameObject hitEffectGO = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(hitEffectGO, 1);
        }
        if (CompareTag("Enemy") && collision.gameObject.CompareTag("Attack") && _iFrames <= 0)
        {
            int dmg = 100;

            dmgText.text = dmg.ToString();

            _curHealth -= dmg;

            _iFrames = iFramesAmount;

            // Hit effect
            GameObject hitEffectGO = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(hitEffectGO, 1);
        }
    }

    private void UpdateUI()
    {
        if (healthSlider == null)
            return;

        healthSlider.fillAmount = (float)_curHealth / maxHealth;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Rendering;

public class healthManager : MonoBehaviour
{
    public float health;

    [Header("Invincibility frames (seconds)")]
    public float iFramesAmount = 3f;


    private float iFrames = 0f;
    private bool colorChanging = false;


    [Header("")]
    public SpriteRenderer sr;


    private void Update()
    {
        if (iFrames > 0)
        {
            iFrames -= Time.deltaTime;
        }
        if(iFrames > 0 && !colorChanging)
        {
            StartCoroutine(ColorChange());
        }
        if (iFrames <= 0)
        {
            StopCoroutine(ColorChange());
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        }



        /*
        
        //+0.5 så de to timere liner up
        if (colorChange < iFramesAmount+0.5f)
        {
            colorChange += Time.deltaTime;

            Color tmp1 = sr.color;
            tmp1.a = colorChange/(iFramesAmount+0.5f);
            sr.color = tmp1;
        }
        */

    }

    private IEnumerator ColorChange()
    {
        colorChanging = true;

        float timeElapsed = 0;
        Color tmp1 = sr.color;

        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime;
            tmp1.a = Mathf.Lerp(0.5f, 1f, timeElapsed / 1f);
            sr.color = tmp1;
            yield return null;
        }

        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);

        float timeElapsed2 = 0;

        while (timeElapsed2 < 1f)
        {
            timeElapsed2 += Time.deltaTime;
            tmp1.a = Mathf.Lerp(1f, 0.5f, timeElapsed2 / 1f);
            sr.color = tmp1;
            yield return null;
        }

        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f);

        colorChanging = false;
    }
//Collision med enemies
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (iFrames <= 0)
            {
                health -= 10f;
                iFrames = iFramesAmount;
            }
           
        }
    }


}

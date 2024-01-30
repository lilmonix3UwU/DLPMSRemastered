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
    private float colorChange = 1f;



    public SpriteRenderer sr;

    private void Update()
    {
        if (iFrames > 0)
        {
            iFrames -= Time.deltaTime;
        }

        //+0.5 så de to timere liner up
        if (colorChange < iFramesAmount+0.5)
        {
            colorChange += Time.deltaTime;

            Color tmp1 = sr.color;
            tmp1.a = colorChange/(iFramesAmount+0.5f);
            sr.color = tmp1;
        }

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

                colorChange = 0.5f;
            }
           
        }
    }


}

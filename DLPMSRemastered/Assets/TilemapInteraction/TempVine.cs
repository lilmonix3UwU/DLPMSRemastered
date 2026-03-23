using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempVine : MonoBehaviour
{
    bool playerNear = false;
    public GameObject particles;
    bool fade = false;
    Color color = new Color(1, 1, 1, 1);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerNear = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerNear = false;
        }
    }
    private void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            particles.SetActive(true);
            fade = true;
            Destroy(gameObject, 2);
        }
    }
    private void FixedUpdate()
    {
        if (fade == true)
        {
            color = new Color(1f, 1f, 1f, color.a - (0.1f * 0.2f));
            Debug.Log(color.a);
            gameObject.GetComponent<SpriteRenderer>().color = color;
        }
    }


}

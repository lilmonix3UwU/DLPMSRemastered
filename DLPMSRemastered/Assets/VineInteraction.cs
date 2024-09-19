using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineInteraction : MonoBehaviour
{
    private void Update()
    {
        if (Physics2D.Raycast(gameObject.transform.position, gameObject.transform.forward, 2, 13))
        {
            RaycastHit2D hit = Physics2D.Raycast(gameObject.transform.position, gameObject.transform.forward, 2, 13);

        }
    }

}

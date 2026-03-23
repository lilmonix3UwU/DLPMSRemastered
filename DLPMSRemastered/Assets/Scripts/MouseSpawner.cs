using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSpawner : MonoBehaviour
{
    [SerializeField] GameObject Mouse;
    GameObject spawnedMouse;
    private void Update()
    {
        if (spawnedMouse == null)
        {
            spawnedMouse = Instantiate(Mouse, transform.position, transform.rotation);
        }
    }

}

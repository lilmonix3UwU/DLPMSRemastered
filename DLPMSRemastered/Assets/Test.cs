using System.Collections;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private GameObject sprite;
    [SerializeField] private ParticleSystem particles;

    private void Start()
    {
        StartCoroutine(WaitForIt());
    }

    private IEnumerator WaitForIt()
    {
        yield return new WaitForSeconds(1.5f);
        Play();
    }

    private void Play()
    {
        sprite.SetActive(false);
        particles.Emit(9999);
    }
}

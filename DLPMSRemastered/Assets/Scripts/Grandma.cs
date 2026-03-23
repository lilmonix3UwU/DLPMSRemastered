using UnityEngine;

public class Grandma : MonoBehaviour
{
    private AudioManager _audio;

    private void Start()
    {
        _audio = AudioManager.Instance;
    }

    private void Footstep()
    {
        _audio.Play(RandomFootstepSound());
    }

    private string RandomFootstepSound()
    {
        string footstepSound = "";

        switch (Random.Range(0, 3))
        {
            case 0:
                footstepSound = "Land1";
                break;
            case 1:
                footstepSound = "Land2";
                break;
            case 2:
                footstepSound = "Land3";
                break;
        }

        return footstepSound;
    }
}

using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private Sound[] sounds;

    public static AudioManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Play("Ambience");
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        
        if (s == null)
            return;

        AudioSource src = gameObject.AddComponent<AudioSource>();

        src.clip = s.clip;
        src.volume = s.volume;
        src.pitch = s.pitch;
        src.time = s.start;
        src.loop = s.loop;
        
        src.Play();
        
        if (s.stopAfter > 0)
            Destroy(src, s.stopAfter);
    }
    
    public void Stop(string name)
    {
        AudioSource[] srces = gameObject.GetComponentsInChildren<AudioSource>();

        AudioSource audioSource = Array.Find(srces, a => a.clip.ToString().Contains(name));

        if (audioSource == null)
            return;

        Destroy(audioSource);
    }
}

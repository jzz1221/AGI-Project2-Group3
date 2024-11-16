using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManagement : MonoBehaviour
{
    public static SoundManagement instance;
    private AudioSource audioSource;
    //Audio Source Path dictionary
    private Dictionary<string, AudioClip> dictAudio;

    void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
        dictAudio = new Dictionary<string, AudioClip>();
    }
    
    void Update()
    {
        
    }
    
    public AudioClip LoadAudio(string path)
    {
        return (AudioClip)Resources.Load(path);
    }

    private AudioClip GetAudio(string path)
    {
        if (!dictAudio.ContainsKey(path))
        {
            dictAudio.Add(path, Resources.Load(path) as AudioClip);
        }
        return dictAudio[path];
    }

    public void PlayBackgroundMusic(string path, float volume = 1.0f)
    {
        audioSource.Stop();
        audioSource.clip = GetAudio(path);
        audioSource.Play();
    }

    public void StopBackgroundMusic()
    {
        audioSource.Stop();
    }

    public void PlayEffectSound(string path, float volume = 1.0f)
    {
        this.audioSource.PlayOneShot(LoadAudio(path));
        this.audioSource.volume = volume;
    }

    public void PlayObjectSound(AudioSource audioSource, string path, float volume = 1.0f)
    {
        audioSource.PlayOneShot(LoadAudio(path));
        audioSource.volume = volume;
    }
}

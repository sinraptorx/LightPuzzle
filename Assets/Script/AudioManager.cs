using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager AM;

    public AudioSource audioSrc;
    public AudioClip[] clips;
    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
    }

    void Awake()
    {
        AM = this;
    }

    public void StageStart()
    {
        audioSrc.PlayOneShot(clips[0]);
    }

    public void Place()
    {
        audioSrc.PlayOneShot(clips[1]);
    }
    
    public void Delete()
    {
        audioSrc.PlayOneShot(clips[2]);
    }

    public void Connect()
    {
        audioSrc.PlayOneShot(clips[3]);
    }

    public void StageClear()
    {
        audioSrc.PlayOneShot(clips[4]);
    }
}

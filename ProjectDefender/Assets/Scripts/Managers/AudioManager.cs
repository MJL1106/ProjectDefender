using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("BGM Details")] [SerializeField]
    private bool playBgm;
    [SerializeField] private AudioSource[] bgm;

    private int currentBgmIndex;

    private void Awake()
    {
        if (instance == null) 
            instance = this;
        else
            Destroy(this.gameObject);
        
        InvokeRepeating(nameof(PlayMusicIfNeeded), 0, 2);
    }
    
    public void PlaySFX(AudioSource audioToPlay)
    {
        if (audioToPlay.clip == null)
        {
            Debug.Log("Could not play " + audioToPlay.gameObject.name + ". THere is no audio Clip assigned");
            return;
        }
        
        if (audioToPlay.isPlaying) audioToPlay.Stop();

        audioToPlay.Play();
    }

    private void PlayMusicIfNeeded()
    {
        if (bgm.Length <= 0)
        {
            Debug.Log("Trying to play music without assigning any");
            return;
        }
        
        if (playBgm == false) return;
        
        if (bgm[currentBgmIndex].isPlaying == false) PlayRandomBGM();
    }

    [ContextMenu("Play Random music")]
    public void PlayRandomBGM()
    {
        currentBgmIndex = Random.Range(0, bgm.Length);
        PlayBGM(currentBgmIndex);
    }
    
    public void PlayBGM(int bgmToPlay)
    {
        if (bgm.Length <= 0)
        {
            Debug.Log("Trying to play music without assigning any");
            return;
        }
        
        StopAllBGM();

        currentBgmIndex = bgmToPlay;
        bgm[bgmToPlay].Play();
    }

    [ContextMenu("Stop all music")]
    public void StopAllBGM()
    {
        for (int i = 0; i < bgm.Length; i++)
        {
            bgm[i].Stop();
        }
    }
}

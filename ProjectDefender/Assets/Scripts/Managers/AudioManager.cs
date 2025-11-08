using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

/// <summary>
/// Singleton manager for all game audio.
/// Handles BGM playback, 3D SFX, and advanced SFX limiting (cooldown, concurrent, per-tower).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("BGM Details")] 
    [SerializeField] private bool playBgm;
    [SerializeField] private AudioSource[] bgm;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup; // Mixer group for all spawned one-shot SFX

    private int currentBgmIndex;

    // Global SFX limiting
    private Dictionary<string, float> lastPlayTime = new Dictionary<string, float>();
    private Dictionary<string, int> concurrentSounds = new Dictionary<string, int>();
    private Dictionary<string, HashSet<int>> activeTowerInstances = new Dictionary<string, HashSet<int>>();

    private void Awake()
    {
        if (instance == null) 
            instance = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }
        
        InvokeRepeating(nameof(PlayMusicIfNeeded), 0, 2);
    }
    
    /// <summary>
    /// Checks if a specific tower instance can play a sound.
    /// Used to limit the number of identical towers playing sounds simultaneously.
    /// </summary>
    public bool CanTowerPlaySound(string towerTypeId, int towerInstanceId, int maxConcurrentTowers)
    {
        if (!activeTowerInstances.ContainsKey(towerTypeId)) activeTowerInstances[towerTypeId] = new HashSet<int>();
        
        if (activeTowerInstances[towerTypeId].Count >= maxConcurrentTowers 
            && !activeTowerInstances[towerTypeId].Contains(towerInstanceId)) return false;
        
        return true;
    }
    
    /// <summary>
    /// Registers that a tower has started playing a sound.
    /// Automatically unregisters it after the sound's duration.
    /// </summary>
    public void RegisterTowerSound(string towerTypeId, int towerInstanceId, float duration)
    {
        if (!activeTowerInstances.ContainsKey(towerTypeId)) activeTowerInstances[towerTypeId] = new HashSet<int>();
        
    
        activeTowerInstances[towerTypeId].Add(towerInstanceId);
        StartCoroutine(UnregisterTowerSoundCo(towerTypeId, towerInstanceId, duration));
    }

    private IEnumerator UnregisterTowerSoundCo(string towerTypeId, int towerInstanceId, float duration)
    {
        yield return new WaitForSeconds(duration);
    
        if (activeTowerInstances.ContainsKey(towerTypeId)) activeTowerInstances[towerTypeId].Remove(towerInstanceId);
        
    }
    
    /// <summary>
    /// Plays a sound from a pre-existing AudioSource component.
    /// Use for looping or persistent sounds.
    /// </summary>
    public void PlaySFX(AudioSource audioToPlay, bool randomPitch = false)
    {
        if (audioToPlay.clip == null)
        {
            Debug.Log("Could not play " + audioToPlay.gameObject.name + ". There is no audio Clip assigned");
            return;
        }
        
        if (audioToPlay.isPlaying) audioToPlay.Stop();
        
        audioToPlay.pitch = randomPitch ? Random.Range(.9f, 1.1f) : 1;
        audioToPlay.Play();
    }

    /// <summary>
    /// Plays a one-shot 3D sound at a specific position.
    /// Creates and destroys a temporary audio source.
    /// </summary>
    public void PlaySFXOneShot(AudioClip clip, Vector3 position, bool randomPitch = false, float volume = 1f)
    {
        if (clip == null)
        {
            Debug.Log("Could not play audio clip - clip is null");
            return;
        }

        CreateAndPlayTemporaryAudio(clip, position, randomPitch, volume);
    }

    /// <summary>
    /// Plays a one-shot 3D sound, but only if it's not on cooldown.
    /// Also limits the max concurrent instances of this sound.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    /// <param name="position">The world-space position to play the sound at.</param>
    /// <param name="soundId">A unique string key to identify this sound for limiting purposes.</param>
    /// <param name="cooldown">The minimum time in seconds before this soundId can be played again.</param>
    /// <param name="maxConcurrent">The maximum number of this soundId that can play at once.</param>
    /// <param name="randomPitch">If true, applies a slight random pitch.</param>
    /// <param name="volume">The volume of the sound (0-1).</param>
    public void PlaySFXOneShotLimited(AudioClip clip, Vector3 position, string soundId, float cooldown = 0.2f, int maxConcurrent = 4, bool randomPitch = false, float volume = 1f)
    {
        if (clip == null) return;

        // Check cooldown
        if (lastPlayTime.ContainsKey(soundId) && Time.time - lastPlayTime[soundId] < cooldown) return;
        
        // Check concurrent limit
        if (!concurrentSounds.ContainsKey(soundId)) concurrentSounds[soundId] = 0;

        if (concurrentSounds[soundId] >= maxConcurrent) return;

        // Play sound
        CreateAndPlayTemporaryAudio(clip, position, randomPitch, volume);
        
        // Update tracking
        lastPlayTime[soundId] = Time.time;
        concurrentSounds[soundId]++;
        
        StartCoroutine(DecreaseConcurrentSoundCo(soundId, clip.length));
    }

    /// <summary>
    /// Creates a temporary GameObject with an AudioSource to play a 3D sound.
    /// </summary>
    private void CreateAndPlayTemporaryAudio(AudioClip clip, Vector3 position, bool randomPitch, float volume)
    {
        GameObject tempAudio = new GameObject("TempAudio_" + clip.name);
        tempAudio.transform.position = position;
    
        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = randomPitch ? Random.Range(.9f, 1.1f) : 1;
        audioSource.outputAudioMixerGroup = sfxMixerGroup;
    
        // 3D audio settings
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 30f;
        audioSource.dopplerLevel = 0f;
        audioSource.spread = 0f;
    
        audioSource.Play();
    
        Destroy(tempAudio, clip.length + 0.1f);
    }

    private IEnumerator DecreaseConcurrentSoundCo(string soundId, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (concurrentSounds.ContainsKey(soundId)) concurrentSounds[soundId] = Mathf.Max(0, concurrentSounds[soundId] - 1);
        
    }

    /// <summary>
    /// Smoothly fades out a playing AudioSource then stops it.
    /// </summary>
    public void FadeOutSFX(AudioSource audioToFade, float fadeTime = 0.2f)
    {
        if (audioToFade != null && audioToFade.isPlaying) StartCoroutine(FadeOutCo(audioToFade, fadeTime));
        
    }

    private IEnumerator FadeOutCo(AudioSource audio, float fadeTime)
    {
        float startVolume = audio.volume;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            audio.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
            yield return null;
        }

        audio.Stop();
        audio.volume = startVolume;
    }

    /// <summary>
    /// Checks if BGM should be playing and starts a new track if needed.
    /// Called by InvokeRepeating.
    /// </summary>
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

    /// <summary>
    /// Selects and plays a random BGM track from the list.
    /// </summary>
    public void PlayRandomBGM()
    {
        currentBgmIndex = Random.Range(0, bgm.Length);
        PlayBGM(currentBgmIndex);
    }
    
    /// <summary>
    /// Plays a specific BGM track by its index.
    /// </summary>
    public void PlayBGM(int bgmToPlay)
    {
        if (bgm.Length <= 0)
        {
            Debug.Log("Trying to play music without assigning any");
            return;
        }
        
        StopAllBGM();

        currentBgmIndex = bgmToPlay;
        bgm[bgmToPlay].loop = true;
        bgm[bgmToPlay].Play();
    }

    /// <summary>
    /// Stops all BGM tracks currently playing.
    /// </summary>
    public void StopAllBGM()
    {
        for (int i = 0; i < bgm.Length; i++)
        {
            bgm[i].Stop();
        }
    }
}
using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Canais de Áudio")]
    public AudioSource musicSourceA; 
    public AudioSource musicSourceB; 
    public AudioSource sfxSource;    

    [Header("Configurações Iniciais")]
    [Range(0, 1)] public float musicVolume = 0.5f; // Começa na metade pra não estourar ouvido
    [Range(0, 1)] public float sfxVolume = 1f;
    public float crossfadeDuration = 2.0f;

    private bool isPlayingA = true; 
    private Coroutine crossfadeRoutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (!musicSourceA) musicSourceA = gameObject.AddComponent<AudioSource>();
        if (!musicSourceB) musicSourceB = gameObject.AddComponent<AudioSource>();
        if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();

        musicSourceA.loop = true;
        musicSourceB.loop = true;
        
        // Garante que o AudioSource obedeça o volume inicial
        musicSourceA.volume = 0; // Começa mudo para fade in
        musicSourceB.volume = 0;
        sfxSource.volume = sfxVolume;
    }

    public void PlayMusic(AudioClip newClip)
    {
        if (newClip == null) return;

        AudioSource activeSource = isPlayingA ? musicSourceA : musicSourceB;
        
        // Se já está tocando e tem volume, ignora
        if (activeSource.clip == newClip && activeSource.isPlaying) 
        {
            // Garante que o volume esteja certo (caso tenha começado mudo)
            activeSource.volume = musicVolume; 
            return;
        }

        if (crossfadeRoutine != null) StopCoroutine(crossfadeRoutine);
        crossfadeRoutine = StartCoroutine(DoCrossfade(newClip));
    }

    IEnumerator DoCrossfade(AudioClip newClip)
    {
        AudioSource outgoing = isPlayingA ? musicSourceA : musicSourceB;
        AudioSource incoming = isPlayingA ? musicSourceB : musicSourceA;

        incoming.clip = newClip;
        incoming.Play();
        incoming.volume = 0f;

        float timer = 0f;
        while (timer < crossfadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / crossfadeDuration;

            outgoing.volume = Mathf.Lerp(musicVolume, 0f, progress);
            incoming.volume = Mathf.Lerp(0f, musicVolume, progress);

            yield return null;
        }

        outgoing.Stop();
        outgoing.volume = 0f;
        incoming.volume = musicVolume;
        
        isPlayingA = !isPlayingA;
    }

    public void SetMusicVolume(float vol)
    {
        musicVolume = vol;
        // Atualiza IMEDIATAMENTE quem estiver tocando
        if (isPlayingA) musicSourceA.volume = musicVolume;
        else musicSourceB.volume = musicVolume;
    }

    public void SetSFXVolume(float vol)
    {
        sfxVolume = vol;
        sfxSource.volume = sfxVolume;
    }
}
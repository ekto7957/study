using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


using System;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    private Dictionary<string, AudioClip> bgmClips = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();

    [System.Serializable]
    public struct NamedAudioClip
    {
        public string name;
        public AudioClip clip;
    }

    public NamedAudioClip[] bgmClipList;
    public NamedAudioClip[] sfxClipList;

    private Coroutine currentBGMCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioClips();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //현재씬의 --- 이름 불러와서 OnSceneLoaded하기 
    // 상황에 맞게 아래코드 수정해서 사용
    //string activeSceneName = SceneManager.GetActiveScene().name;
    //OnSceneLoaded(activeSceneName);

    void InitializeAudioClips()
    {
        foreach (var bgm in bgmClipList)
        {
            if (!bgmClips.ContainsKey(bgm.name))
            {
                bgmClips.Add(bgm.name, bgm.clip);
            }
        }
        foreach (var sfx in sfxClipList)
        {
            if (!sfxClips.ContainsKey(sfx.name))
            {
                sfxClips.Add(sfx.name, sfx.clip);
            }
        }
    }

    public void PlayBGM(string name, float fadeDuration = 1.0f)
    {
        if (bgmClips.ContainsKey(name))
        {

            if(currentBGMCoroutine != null)
            {
                StopCoroutine(currentBGMCoroutine);
            }

            currentBGMCoroutine = StartCoroutine(FadeOutBGM(fadeDuration, () =>
            {
                bgmSource.clip = bgmClips[name];
                bgmSource.Play();
                currentBGMCoroutine = StartCoroutine(FadeInBGM(fadeDuration));

            }));
        }
    }

    public void PlaySfx(string name, Vector3 position)
    {
        if (sfxClips.ContainsKey(name))
        {
            //sfxSource.PlayOneShot(sfxClips[name]);
            AudioSource.PlayClipAtPoint(sfxClips[name], position);  // 특정 포지션에서 사운드를 재생해줌 !
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void StopSFX()
    {
        sfxSource.Stop();
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp(volume, 0, 1);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp(volume, 0, 1);
    }

    private IEnumerator FadeOutBGM(float duration, Action onFadeComplete)
    {
        float startVolume = bgmSource.volume;

        for(float t = 0; t < duration; t++)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        bgmSource.volume = 0;

        onFadeComplete?.Invoke(); // 페이드 아웃이 완료되면 다음 작업 실행

    }

    private IEnumerator FadeInBGM(float duration)
    {
        float startVolume = 0f;
        bgmSource.volume = 0f;

        for(float t = 0; t < duration ; t+= Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume , 1f, t / duration);
            yield return null;
        }

        bgmSource.volume = 1.0f; 
    }
}
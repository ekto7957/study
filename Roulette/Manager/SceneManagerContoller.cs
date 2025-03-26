using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneManagerController : MonoBehaviour
{
    public static SceneManagerController Instance { get; private set; }

    public Image panel;
    public float fadeDuration = 1.0f;
    public string nextSceneName;
    private bool isFading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        SoundManager.Instance.SetSFXVolume(0.5f);
        //SoundManager.Instance.PlaySfx("ItemGetSound");

        SceneManager.LoadScene(sceneName);

        Debug.Log("Scene º¯°æ : " + sceneName);
    }

    public void ExitScene()
    {
        Application.Quit();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && !isFading)
        {
            StartCoroutine(FadeInAndLoadScene());
        }
    }

    IEnumerator FadeInAndLoadScene()
    {
        isFading = true;

        yield return StartCoroutine(FadeImage(0, 1, fadeDuration));

        yield return StartCoroutine(FadeImage(1, 0, fadeDuration));

        isFading = false;
    }

    IEnumerator FadeImage(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0.0f;

        Color panelColor = panel.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            panelColor.a = newAlpha;
            panel.color = panelColor;
            yield return null;
        }
        panelColor.a = endAlpha;
        panel.color = panelColor;

        if (isFading)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
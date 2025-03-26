using UnityEngine;
using UnityEngine.UI;

public class GameSettingUIManager : MonoBehaviour
{
    public GameObject SettingsObj;

    public Text resolutionText
        ;
    public Text graphiscsQulityText;

    public Text fullScreenText;

    private int resolutionIndex = 0;
    private int qualityIndex = 0;
    private bool isFullScreen = true;

    private string[] resolutions = { "800 x 600", "1280 x 720", "1920 x 1080" };
    private string[] qualityOptions = { "Low", "Medium", "High" };

    private void Start()
    {
        resolutionText.text = resolutions[resolutionIndex];
        graphiscsQulityText.text = qualityOptions[qualityIndex];
        fullScreenText.text = isFullScreen ? "Full Screen" : "Windowed";
    }

    public void OnResolutionLeftClick()
    {
        resolutionIndex = Mathf.Max(0, resolutionIndex - 1);
    }

    public void OnResolutionRightClick()
    {
        resolutionIndex = Mathf.Min(resolutions.Length - 1, resolutionIndex + 1);


    }

    public void OnGraphicsLeftClick()
    {
        qualityIndex = Mathf.Max(0, qualityIndex - 1);
    }

    public void OnGraphicsRightClick()
    {
        qualityIndex = Mathf.Min(qualityOptions.Length - 1, qualityIndex + 1);
    }

    public void OnFullScreenClick()
    {
        isFullScreen = !isFullScreen;
        fullScreenText.text = isFullScreen ? "Full Screen" : "Windowed";
    }

    private void UpdateResolutionText()
    {
        resolutionText.text = resolutions[resolutionIndex];
    }

    private void UpdateGraphicsQualityText()
    {
        graphiscsQulityText.text = qualityOptions[qualityIndex];
    }

    private void updateFullScreenText()
    {
        fullScreenText.text = isFullScreen ? "Full Screen" : "Windowed";
    }

    public void OnApllySettingsClick()
    {
        SoundManager.Instance.PlaySfx("OnSettings", transform.position);
        ApplySettings();
        SaveSettings();
    }

    private void ApplySettings()
    {
        SoundManager.Instance.PlaySfx("OnSettings", transform.position);
        string[] res = resolutions[resolutionIndex].Split('x');
        int width = int.Parse(res[0]);
        int height = int.Parse(res[1]);
        Screen.SetResolution(width, height, isFullScreen);
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.SetInt("GraphicsQualityIndex", qualityIndex);
        PlayerPrefs.SetInt("FullScreen", isFullScreen ? 1 : 0);
        PlayerPrefs.Save();

    }

    private void LoadSettings()
    {
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 1);
        qualityIndex = PlayerPrefs.GetInt("GraphicsQualityIndex", 1);
        //isFullScreen = PlayerPrefs.GetInt("FullScreen", 1) == 1;
    }

    public void OnSettings()
    {

    }
}

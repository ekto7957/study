using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    //�ε� Slider UI�� �������� ����
    public Slider loadingSlider;

    IEnumerator LoadLoadingSceneAndNextScene() //�ε� ���� �ε��ϰ� NextScene�� �ε�� ������ ����ϴ� �ڷ�ƾ
    {
        //�ε� ���� �񵿱������� �ε�(�ε� ����ǥ�ÿ����� ����ϴ� ��)
        AsyncOperation loadingSceneOp = SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
        loadingSceneOp.allowSceneActivation = false;

        while (!loadingSceneOp.isDone) //�ε����� �ε�� ������ ���
        {
            if (loadingSceneOp.progress >= 0.9f)
            {
                loadingSceneOp.allowSceneActivation = true; //�ε��� �غ� �Ϸ�Ǹ� �� Ȱ��ȭ
            }
            yield return null;
        }
        FindLoadingSliderInScene(); //�ε� ������ �ε� Slider�� ã�ƿ���

        AsyncOperation nextSceneOp = SceneManager.LoadSceneAsync("Menu");//NextScene�� �񵿱������� �ε�
        while (!nextSceneOp.isDone) //�ε� ������� Slider�� ǥ��
        {
            loadingSlider.value = nextSceneOp.progress; //�ε����൵ ������Ʈ(0~1)
            yield return null;
        }
        SceneManager.UnloadSceneAsync("LoadingScene"); //NextScene�� ������ �ε�� ��, �ε����� ��ε�

    }

    void FindLoadingSliderInScene()
    {
        loadingSlider = GameObject.Find("LoadingSlider").GetComponent<Slider>();
    }
}
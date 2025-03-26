using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    //로딩 Slider UI를 가져오는 변수
    public Slider loadingSlider;

    IEnumerator LoadLoadingSceneAndNextScene() //로딩 씬을 로드하고 NextScene이 로드될 때까지 대기하는 코루틴
    {
        //로딩 씬을 비동기적으로 로드(로딩 상태표시용으로 사용하는 씬)
        AsyncOperation loadingSceneOp = SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
        loadingSceneOp.allowSceneActivation = false;

        while (!loadingSceneOp.isDone) //로딩씬이 로드될 때까지 대기
        {
            if (loadingSceneOp.progress >= 0.9f)
            {
                loadingSceneOp.allowSceneActivation = true; //로딩씬 준비 완료되면 씬 활성화
            }
            yield return null;
        }
        FindLoadingSliderInScene(); //로딩 씬에서 로딩 Slider를 찾아오기

        AsyncOperation nextSceneOp = SceneManager.LoadSceneAsync("Menu");//NextScene을 비동기적으로 로드
        while (!nextSceneOp.isDone) //로딩 진행률을 Slider에 표시
        {
            loadingSlider.value = nextSceneOp.progress; //로딩진행도 업데이트(0~1)
            yield return null;
        }
        SceneManager.UnloadSceneAsync("LoadingScene"); //NextScene이 완전히 로드된 후, 로딩씬을 언로드

    }

    void FindLoadingSliderInScene()
    {
        loadingSlider = GameObject.Find("LoadingSlider").GetComponent<Slider>();
    }
}
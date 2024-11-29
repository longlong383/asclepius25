using Microsoft.MixedReality.Toolkit.SceneSystem;
using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    private async void Awake()
    {
        IMixedRealitySceneSystem sceneSystem = MixedRealityToolkit.Instance.GetService<IMixedRealitySceneSystem>();
        //load startup as single scene
        await sceneSystem.LoadContent("handAnnotationDemo", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public async void LoadSceneSingle(string sceneName)
    {
        IMixedRealitySceneSystem sceneSystem = MixedRealityToolkit.Instance.GetService<IMixedRealitySceneSystem>();
        await sceneSystem.LoadContent(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public async void LoadSceneAdditive(string sceneName)
    {
        IMixedRealitySceneSystem sceneSystem = MixedRealityToolkit.Instance.GetService<IMixedRealitySceneSystem>();
        await sceneSystem.LoadContent(sceneName);
    }
}

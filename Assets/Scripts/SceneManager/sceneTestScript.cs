using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sceneTestScript : MonoBehaviour
{
    public Interactable interactable;
    [SerializeField] string sceneChange;
    SceneManager sceneManager;
    // Start is called before the first frame update
    void Start()
    {
        interactable.OnClick.AddListener(()=>sceneManager.LoadSceneSingle(sceneChange));
    }
}

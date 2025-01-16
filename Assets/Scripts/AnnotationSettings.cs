using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine.UI;
using System.Drawing;

//used for turning toggles on and off, specifically the arrows and the start and end blocks for the annotations
//removed arrows as they are too distracting for precise annotations
public class AnnotationSettings : MonoBehaviour
{
    public GameObject /*verticeHolder,*/ startEndHolder;
    public Interactable /*toggleVertice,*/ toggleStartEnd;
    private BooleanSync booleanSync;
    // Start is called before the first frame update
    void Start()
    {
        // toggleVertice.OnClick.AddListener(() => showVertices(toggleVertice.IsToggled));
        toggleStartEnd.OnClick.AddListener(() => showStartEnd(toggleStartEnd.IsToggled));
        destroyer.OnClick.AddListener(() => deleteAllAnnotations());
        //toggleVertice.IsToggled = true;
        toggleStartEnd.IsToggled = true;
        destroyer.IsToggled = true;
        if(FindObjectsOfType<BooleanSync>() != null)
        {
            booleanSync = FindObjectOfType<BooleanSync>();
        }
    }
    
    // Update is called once per frame
    void showVertices(bool placeHolder)
    {
        verticeHolder.SetActive(placeHolder);
        if (booleanSync.returnIsConnected() == true)
        {
            Debug.Log(placeHolder);
            booleanSync.setArrows(placeHolder);
        }
    }
    
    // Update is called once per frame
    //not needed anymore
    // void showVertices(bool placeHolder)
    // {
    //     verticeHolder.SetActive(placeHolder);
    // }

    void showStartEnd(bool placeHolder)
    {
        startEndHolder.SetActive(placeHolder);
        if (booleanSync.returnIsConnected() == true)
        {
            Debug.Log(placeHolder);
            booleanSync.setStartEndBlock(placeHolder);
        }
    }

    void deleteAllAnnotations()
    {
        if (FindObjectOfType<AnnotationController>() != null && booleanSync.returnIsConnected() == true)
        {
            AnnotationController annotationController = FindObjectOfType<AnnotationController>();
            annotationController.callDestroyEverything();
            booleanSync.setDeletion(true);
        }
    }

}

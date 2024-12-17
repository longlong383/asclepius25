using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine.UI;
using System.Drawing;

//used for turning toggles on and off, specifically the arrows and the start and end blocks for the annotations
public class AnnotationSettings : MonoBehaviour
{
    public GameObject verticeHolder, startEndHolder;
    public Interactable toggleVertice, toggleStartEnd, destroyer;
    private BooleanSync booleanSync;
    // Start is called before the first frame update
    void Start()
    {
        toggleVertice.OnClick.AddListener(() => showVertices(toggleVertice.IsToggled));
        toggleStartEnd.OnClick.AddListener(() => showStartEnd(toggleStartEnd.IsToggled));
        destroyer.OnClick.AddListener(() => deleteAllAnnotations(destroyer.IsToggled));
        toggleVertice.IsToggled = true;
        toggleStartEnd.IsToggled = true;
        destroyer.IsToggled = true;

        if(FindObjectsOfType<BooleanSync>() != null)
        {
            booleanSync = FindObjectOfType<BooleanSync>();
        }
    }

    //makes arrows along annotation appear/disappear
    void showVertices(bool placeHolder)
    {
        Debug.Log(placeHolder);
        verticeHolder.SetActive(placeHolder);
        if (booleanSync.returnIsConnected() == true)
        {
            Debug.Log(placeHolder);
            //sets bool value on photon network
            booleanSync.setArrows(placeHolder);
        }
    }

    //makes start/end cubes appear/disappear
    void showStartEnd(bool placeHolder)
    {
        Debug.Log(placeHolder);
        startEndHolder.SetActive(placeHolder);
        if (booleanSync.returnIsConnected() == true)
        {
            Debug.Log(placeHolder);
            //sets bool value on photon network
            booleanSync.setStartEndBlock(placeHolder);
        }
    }

    //deletes all annotations in scene
    void deleteAllAnnotations(bool placeHolder)
    {
        Debug.Log(placeHolder);
        if (FindObjectOfType<AnnotationController>() != null && booleanSync.returnIsConnected() == true)
        {
            AnnotationController annotationController = FindObjectOfType<AnnotationController>();
            annotationController.callDestroyEverything();
            //sets deletion bool value on script
            booleanSync.setDeletion(placeHolder);
        }
    }

}

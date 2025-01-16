using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using UnityEngine;
//used for changing the colors of the annotations
//(or more specifically changing the type of annotation)
public class lineRendererColor : MonoBehaviour
{
    //getting colors from the color of the icon buttons
    private List <Material> lineColor = new List<Material>();
    //gameobject that holds all the buttons int he color panel
    public GameObject parentButton;
    //accesing the interactable components of each button
    private List<Interactable> colorButtons = new List<Interactable>();
    //used for accessing the public variables in the annotationController
    public AnnotationController annotationController;
    // Start is called before the first frame update

    private BooleanSync booleanSync;

    void Start()
    {
        //accessing each color button
        foreach (Transform button in parentButton.transform)
        {
            Interactable temp = button.gameObject.GetComponent<Interactable>();
            if (temp == null)
            {
                Debug.Log("error adding color buttons");
            }
            colorButtons.Add(temp);
            //accesing the quad with the color material
            Transform temp1 = temp.transform.Find("BackPlate");
            Transform temp2 = temp1.transform.Find("Quad");
            //adding method specific to each panel
            button.GetComponent<Interactable>().OnClick.AddListener(() => lineColorChange(temp2.GetComponent<Renderer>().material, button.name));
        }
        //assigning default annotation type general correction for the csv
        annotationController.annotationName = colorButtons[0].name;

        if (FindObjectOfType<BooleanSync>() == null)
        {
            Debug.Log("Error retrieving booleanSync script");
        }
        else
        {
            booleanSync = FindObjectOfType<BooleanSync>();
        }
    }
      
    private void lineColorChange(Material lineMaterial, string annotationType)
    {
        //setting each button up such that it changes the annotation material color based on the button clicked
        //updates annotationController variable
        if (annotationController.draw != true && booleanSync.returnIsConnected() == true)
        {
            annotationController.lineRend.GetComponent<LineRenderer>().material = lineMaterial;
            annotationController.annotationName = annotationType;
            booleanSync.setAnnotationType(annotationType);
        }
        else
        {
            annotationController.Debug1.text += "\n Please stop drawing first";
        }
        
    }

}

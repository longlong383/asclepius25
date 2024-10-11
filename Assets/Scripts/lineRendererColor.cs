using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using UnityEngine;

public class lineRendererColor : MonoBehaviour
{
    private List <Material> lineColor = new List<Material>();
    public GameObject parentButton;
    private List<Interactable> colorButtons = new List<Interactable>();
    public AnnotationController annotationController;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform button in parentButton.transform)
        {
            Interactable temp = button.gameObject.GetComponent<Interactable>();
            if (temp == null)
            {
                Debug.Log("error adding color buttons");
            }
            colorButtons.Add(temp);
            Transform temp1 = temp.transform.Find("BackPlate");
            Transform temp2 = temp1.transform.Find("Quad");
            button.GetComponent<Interactable>().OnClick.AddListener(() => lineColorChange(temp2.GetComponent<Renderer>().material, button.name));
        }
        //assigning default annotation type general correction for the csv
        annotationController.annotationName = colorButtons[0].name;
    }
      
    private void lineColorChange(Material lineMaterial, string annotationType)
    {
        if (annotationController.draw != true)
        {
            annotationController.lineRend.GetComponent<LineRenderer>().material = lineMaterial;
            annotationController.annotationName = annotationType;
        }
        else
        {
            annotationController.Debug1.text += "\n Please stop drawing first";
        }
        
    }

}

using Microsoft.MixedReality.Toolkit.UI;
using Newtonsoft.Json.Serialization;
using System.Collections;
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
            if (temp != null)
            {
                Debug.Log("error adding color buttons");
            }
            colorButtons.Add(temp);
            Transform temp1 = temp.transform.Find("BackPlate");
            Transform temp2 = temp1.transform.Find("Quad");
            button.GetComponent<Interactable>().OnClick.AddListener(() => lineColorChange(temp2.GetComponent<Renderer>().material));
        }
    }
      
    private void lineColorChange(Material lineMaterial)
    {
        if (annotationController.draw != true)
        {
            annotationController.lineRend.GetComponent<LineRenderer>().material = lineMaterial;
            annotationController.Debug1.text += "\n Please stop drawing first";
        }
        
    }

}

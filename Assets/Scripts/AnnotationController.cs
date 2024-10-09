using System.Collections;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using TMPro;
using UnityEngine;


public class AnnotationController : MonoBehaviour, IMixedRealitySpeechHandler
{
    public TMP_Text Debug1;
    public GameObject parentHolderBall,parentHolderLineRenderer;

    public GameObject annotationObject, startEndBlock;

    private bool startBlockBool;

    public bool draw;

    public Material lineMaterial, startMaterial, endMaterial;

    public GameObject lineRend;

    void Start()
    {
        draw = false;
        CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this);
        startBlockBool = false;
        Debug1.text += "\nStarting Annotation System";
    }

    //method is only used for debugging purposes, to be commented out during gameplay on hololens
    //void Update()
    //{
    //}

    public void OnSpeechKeywordRecognized(SpeechEventData eventData)
    {
        switch (eventData.Command.Keyword.ToLower())
        {
            case "start to draw":
                if (!draw)
                {
                    Debug1.text += "\nStarting to draw";
                    Debug.Log("Starting to draw");
                    draw = true;
                    startBlockBool = true;
                    StartCoroutine(InstantiateCoroutine());
                }
                break;

            case "stop to draw":
                endBlock();
                break;

            case "reset":
                Debug1.text += "\nResetting";
                Debug.Log("Resetting");
                endBlock();
                foreach (Transform child in parentHolderBall.transform)
                {
                    Destroy(child.gameObject);
                }
                foreach (Transform line in parentHolderLineRenderer.transform)
                {
                    Destroy(line.gameObject);
                }
                
                break;

            default:
                Debug.Log($"Unknown option {eventData.Command.Keyword}");
                break;
        }
    }

    private IEnumerator InstantiateCoroutine()
    {
        Debug1.text += "\nFirst Point";

        GameObject temp = Instantiate(lineRend);
        temp.transform.SetParent(parentHolderLineRenderer.transform);
        LineRenderer lineRenderer1 = temp.GetComponent<LineRenderer>();

        while (draw)
        {
            // Reset the frame count
            int frameCount = 0;
        
            // Wait for # frames before instantiating the object
            while (frameCount < 20)
            {
                frameCount++;
                yield return null; // Wait for the next frame
            }
            Debug.Log("Instantiating annotation");

            if (startBlockBool == true)
            {
                GameObject startBlock = Instantiate(startEndBlock, annotationObject.transform.position, annotationObject.transform.rotation);
                startBlock.SetActive(true);
                startBlock.transform.SetParent(parentHolderBall.transform);
                startBlock.GetComponent<Renderer>().material = startMaterial;
                startBlock.transform.localScale = new Vector3(0.0118f, 0.0118f, 0.0118f);
                startBlockBool = false;
            }
            else
            {
                // Instantiate the annotation object at the parentHolder's position and rotation
                GameObject newAnnotation = Instantiate(annotationObject, annotationObject.transform.position, annotationObject.transform.rotation);
                newAnnotation.transform.SetParent(parentHolderBall.transform);
                newAnnotation.transform.localScale = new Vector3(0.0118f, 0.0118f, 0.0118f);
            }
            
            // Adding Line Renderer component

            // Get the current number of points in the Line Renderer
            int currentPoints = lineRenderer1.positionCount;

            // Increase the point count to accommodate the new point
            lineRenderer1.positionCount = currentPoints + 1;

            // Set the new point at the end of the Line Renderer
            lineRenderer1.SetPosition(currentPoints, annotationObject.transform.position);
        }
    }

    private void endBlock()
    {
        if (draw)
        {
            Debug1.text += "\nStopping to draw";
            Debug.Log("Stopping to draw");
            draw = false;
            Transform lastChild = parentHolderBall.transform.GetChild(parentHolderBall.transform.childCount - 1);
            GameObject endBlock = Instantiate(startEndBlock, annotationObject.transform.position, annotationObject.transform.rotation);
            endBlock.SetActive(true);
            endBlock.transform.SetParent(parentHolderBall.transform);
            endBlock.GetComponent<Renderer>().material = endMaterial;
            endBlock.transform.localScale = new Vector3(0.0118f, 0.0118f, 0.0118f);
            endBlock.transform.position = lastChild.transform.position;
            endBlock.transform.rotation = lastChild.transform.rotation;
            Destroy(lastChild.gameObject);
            StopAllCoroutines();
        }
    }
}

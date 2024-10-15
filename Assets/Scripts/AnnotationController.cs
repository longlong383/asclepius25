using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using TMPro;
using UnityEngine;



public class AnnotationController : MonoBehaviour, IMixedRealitySpeechHandler
{
    public TMP_Text Debug1;
    public GameObject parentHolderBall,parentHolderLineRenderer, startEndHolder;

    public GameObject annotationObject, startEndBlock;

    private bool startBlockBool;

    [HideInInspector] public bool draw;

    public Material lineMaterial, startMaterial, endMaterial;

    public GameObject lineRend;

    [HideInInspector] public string annotationName;

    public List <Transform> thingsToReset = new List<Transform>();

    private List<Transform> storage = new List<Transform>();

    void Start()
    {
        annotationName = null;
        draw = false;
        CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this);
        startBlockBool = false;
        Debug1.text += "\nStarting Annotation System";
        foreach (Transform og in thingsToReset)
        {
            // Instantiate creates a new GameObject, including the Transform, at the same position/rotation/scale
            Transform copy = Instantiate(og, og.position, og.rotation);
            copy.gameObject.SetActive(false);
            // Optionally, add the new copy to a list for later use
            storage.Add(copy);
        }
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
                ExportCoordinatesToCSV();
                break;

            case "reset annotations":
                Debug1.text += "\nResetting";
                Debug.Log("Resetting");
                endBlock();
                destroyEverything();
                break;

            case "reset scene":
                Debug1.text += "\nResetting scene";
                for (int i = 0; i < thingsToReset.Count; i++)
                {
                    thingsToReset[i].position = storage[i].position;
                    thingsToReset[i].rotation = storage[i].rotation;
                    thingsToReset[i].localScale = storage[i].localScale;
                }
                destroyEverything();
                break;

            default:
                Debug.Log($"Unknown option {eventData.Command.Keyword}");
                break;
        }
    }

    private void destroyEverything()
    {
        foreach (Transform child in parentHolderBall.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform line in parentHolderLineRenderer.transform)
        {
            Destroy(line.gameObject);
        }
        foreach (Transform block in startEndHolder.transform)
        {
            Destroy(block.gameObject);
        }
    }
    private IEnumerator InstantiateCoroutine()
    {
        Debug1.text += "\nFirst Point";

        GameObject temp = Instantiate(lineRend);
        temp.transform.SetParent(parentHolderLineRenderer.transform);
        LineRenderer lineRenderer1 = temp.GetComponent<LineRenderer>();
        lineRenderer1.startWidth = 0.002f;
        lineRenderer1.endWidth = 0.002f;

        int frameCountBall = 0;
        while (draw)
        {
            // Reset the frame count
            int frameCount = 0;
        
            // Wait for # frames before instantiating the object
            while (frameCount < 20)
            {
                frameCountBall++;
                frameCount++;
                yield return null; // Wait for the next frame
            }
            Debug.Log("Instantiating annotation");


            // Adding Line Renderer component

            // Get the current number of points in the Line Renderer
            int currentPoints = lineRenderer1.positionCount;

            // Increase the point count to accommodate the new point
            lineRenderer1.positionCount = currentPoints + 1;

            // Set the new point at the end of the Line Renderer
            lineRenderer1.SetPosition(currentPoints, annotationObject.transform.position);

            if (startBlockBool == true)
            {
                GameObject startBlock = Instantiate(startEndBlock, annotationObject.transform.position, annotationObject.transform.rotation);
                startBlock.SetActive(true);
                startBlock.transform.SetParent(startEndHolder.transform);
                startBlock.GetComponent<Renderer>().material = startMaterial;
                startBlock.transform.localScale = new Vector3(0.006f, 0.006f, 0.005f);
                startBlockBool = false;
            }
            else if (frameCountBall == 40) 
            {
                //getting the vector based off the points of the previous two line renderers
                Vector3 direction = lineRenderer1.GetPosition(lineRenderer1.positionCount - 1) - lineRenderer1.GetPosition(lineRenderer1.positionCount - 2);

                //adjusting the direction of the arrow
                Quaternion quaternion = Quaternion.LookRotation(direction);
                Quaternion offset = Quaternion.Euler(0,90,0);
                quaternion = quaternion * offset;

                // Instantiate the annotation object at the parentHolder's position and rotation
                GameObject newAnnotation = Instantiate(annotationObject, annotationObject.transform.position, quaternion);
                newAnnotation.transform.SetParent(parentHolderBall.transform);
                newAnnotation.transform.localScale = new Vector3(0.3f, 0.3f, 0.6f);
                frameCountBall = 0;
            }
        }
    }

    private void endBlock()
    {
        //GameObject temp = Instantiate(lineRend);
        //temp.transform.SetParent(parentHolderLineRenderer.transform);
        Transform temp = parentHolderLineRenderer.transform.GetChild(parentHolderLineRenderer.transform.childCount -1);
        LineRenderer lastLine = temp.GetComponent<LineRenderer>();

        if (draw)
        {
            Debug1.text += "\nStopping to draw";
            Debug.Log("Stopping to draw");
            draw = false;
            Transform lastChild = parentHolderBall.transform.GetChild(parentHolderBall.transform.childCount - 1);
            GameObject endBlock = Instantiate(startEndBlock, annotationObject.transform.position, annotationObject.transform.rotation);
            endBlock.SetActive(true);
            endBlock.transform.SetParent(startEndHolder.transform);
            endBlock.GetComponent<Renderer>().material = endMaterial;
            endBlock.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);

            if (lastChild.transform.position == lastLine.GetPosition(lastLine.positionCount-1))
            {
                endBlock.transform.position = lastChild.transform.position;
                endBlock.transform.rotation = lastChild.transform.rotation;
                Destroy(lastChild.gameObject);
            }
            else
            {
                //getting the vector based off the points of the previous two line renderers
                Vector3 direction = lastLine.GetPosition(lastLine.positionCount - 1) - lastLine.GetPosition(lastLine.positionCount - 2);

                //adjusting the direction of the arrow
                Quaternion quaternion = Quaternion.LookRotation(direction);
                Quaternion offset = Quaternion.Euler(0, 90, 0);
                quaternion = quaternion * offset;
                endBlock.transform.rotation = quaternion;
                endBlock.transform.position = lastLine.GetPosition(lastLine.positionCount - 1);
            }
            
            StopAllCoroutines();
        }
    }

    void ExportCoordinatesToCSV()
    {
        Transform lastChild = parentHolderLineRenderer.transform.GetChild(parentHolderLineRenderer.transform.childCount - 1);

        if (lastChild.GetComponent<LineRenderer>() == null)
        {
            Debug1.text += "\nAn error has occured exporting the CSV";
            return;
        }

        LineRenderer lineRenderer = lastChild.GetComponent<LineRenderer>();

        // Combine it with the filename you want to access
        string filePath = Path.Combine(Application.persistentDataPath, "Annotation_coordinates.csv");

        // Open or create the CSV file
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            // Write header
            writer.WriteLine("Index,X,Y,Z, Annotation Type");

            // Loop through all the positions in the LineRenderer
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                // Get the position at index i
                Vector3 position = lineRenderer.GetPosition(i);

                // Write the index and coordinates to the CSV
                writer.WriteLine($"{i},{position.x},{position.y},{position.z},{annotationName}");
            }
        }

        Debug.Log($"Coordinates exported to {filePath}");
        Debug1.text += $"\nFile Path name: {filePath}";
    }
}

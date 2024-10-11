using System.Collections;
using System.IO;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;



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

    void Start()
    {
        annotationName = null;
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
                ExportCoordinatesToCSV();
                break;

            case "reset annotations":
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
                foreach (Transform block in startEndHolder.transform)
                {
                    Destroy(block.gameObject);
                }
                break;

            case "reset scene":
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        lineRenderer1.startWidth = 0.003f;
        lineRenderer1.endWidth = 0.003f;

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
            else
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
            }
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
            endBlock.transform.SetParent(startEndHolder.transform);
            endBlock.GetComponent<Renderer>().material = endMaterial;
            endBlock.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);
            endBlock.transform.position = lastChild.transform.position;
            endBlock.transform.rotation = lastChild.transform.rotation;
            Destroy(lastChild.gameObject);
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

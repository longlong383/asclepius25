using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;


public class AnnotationController : MonoBehaviour, IMixedRealitySpeechHandler
{
    //debug window text used to display debug messages
    public TMP_Text Debug1;

    //gameobjects used to hold the annotations
    /*
     * parentBallholdr holds the arrows during gameplay
     * parentHolderLineRenderer holds the linerenderer components
     * startend holder holds the start and end points of the annotations
     */
    public GameObject parentHolderBall,parentHolderLineRenderer, startEndHolder;


    //annotationObject holds the arrow prefab that is instantiated along the linerenderer/annotations
    //startendblock holds the cube prefabs used to mark the start and end of an annotation
    public GameObject referenceSphere, annotationObject, startEndBlock;

    //bool used to mark when an annotation is starting 
    private bool startBlockBool;

    //bool used to signify whether or not an annotation is being drawn
    [HideInInspector] public bool draw;

    //materials used to colorcoat the annotations
    public Material lineMaterial, startMaterial, endMaterial, onMaterial, offMaterial;

    //gameobject used to create multiple line renderers
    public GameObject lineRend;

    //string used to mark what type of annotation it is
    [HideInInspector] public string annotationName;

    //gameObjects in scene to reset upon command
    [SerializeField] private Transform body;

    [HideInInspector] private Transform annotationTracker;

    private BooleanSync booleanSync;

    private bool drewAtLeastOneArrow;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private bool positionSetting;

    void Start()
    {
        //setting up variables
        annotationName = null;
        draw = false;
        startBlockBool = false;
        drewAtLeastOneArrow = false;
        annotationTracker = null;

        //accessing speech handlder
        CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this);
        Debug1.text += "\nStarting Annotation System";

        initialPosition = body.position;
        initialRotation = body.rotation;
        initialScale = body.localScale;
        positionSetting = true;
    }

    //voice commands on receival from the MRTK Toolkit
    public void OnSpeechKeywordRecognized(SpeechEventData eventData)
    {
        if (checkConnection() == false)
        {
            return;
        }
        switch (eventData.Command.Keyword.ToLower())
        {
            case "start to draw":
                StartDrawing();
                break;
            case "stop to draw":
                StopDrawing();
                break;
            case "reset annotations":
                ResetAnnotations();
                break;
            case "reset scene":
                ResetScene();
                break;
            case "set position":
                setPosition();
                break;
            default:
                Debug.Log($"Unknown option {eventData.Command.Keyword}");
                break;
        }
    }

    private void setPosition()
    {
        // Toggle the positionSetting boolean
        positionSetting = !positionSetting;

        // Enable or disable the ObjectManipulator based on the new positionSetting value
        body.GetComponent<ObjectManipulator>().enabled = positionSetting;
    }

    // Case-specific methods
    public void StartDrawing()
    {
        if (!draw)
        {
            Debug1.text += "\nStarting to draw";
            Debug.Log("Starting to draw");
            draw = true;
            startBlockBool = true;
            StartCoroutine(InstantiateCoroutine());
        }
    }

    public void StopDrawing()
    {
        endBlock();
        ExportCoordinatesToCSV();
    }

    private void ResetAnnotations()
    {
        Debug1.text += "\nResetting";
        Debug.Log("Resetting");
        endBlock();
        destroyEverything();
    }

    private void ResetScene()
    {
        Debug1.text += "\nResetting scene";
        body.position = initialPosition;
        body.rotation = initialRotation;
        body.localScale = initialScale; 
        referenceSphere.GetComponent<Renderer>().material = offMaterial;
        destroyEverything();
    }

    //destroys all annotations
    private void destroyEverything()
    {
        foreach (Transform child in parentHolderBall.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform line in parentHolderLineRenderer.transform)
        {
            if (line.name.Length >= 8 && line.name.ToLower().Substring(0, 8) == "linerend")
            {
                Destroy(line.gameObject);
            }
        }

        foreach (Transform block in startEndHolder.transform)
        {
            Destroy(block.gameObject);
        }
    }

    public void callDestroyEverything()
    {
        destroyEverything();
    }

    //continuously running function for annotations
    private IEnumerator InstantiateCoroutine()
    {
        if (FindObjectOfType<BooleanSync>() == null)
        {
            StopAllCoroutines();
        }

        booleanSync = FindObjectOfType<BooleanSync>();

        //if no connection to photon server is established, stops annotating immediately
        if (booleanSync.returnIsConnected() == false)
        {
            StopAllCoroutines();
        }

        Debug.Log("boolean status before: " + booleanSync.returnIsDrawing());

        //set boolean as true to let trainee scene know to start tracking annotation tracker
        booleanSync.setIsDrawing(true);
        //mental note, it takes a bit of time for this bool to be sent to the network, hence the time delay is needed to provide
        //an actual reading of the boolean
        yield return new WaitForSeconds(0.1f);
        Debug.Log("boolean status after: " + booleanSync.returnIsDrawing());

        Debug1.text += "\nFirst Point";

        //instantiating new annotating by creating new line renderer
        GameObject temp = Instantiate(lineRend);
        temp.transform.SetParent(parentHolderLineRenderer.transform);
        LineRenderer lineRenderer1 = temp.GetComponent<LineRenderer>();
        lineRenderer1.startWidth = 0.002f;
        lineRenderer1.endWidth = 0.002f;
        //int value used to mark when 40 frames have been achieved
        int frameCountBall = 0;
        referenceSphere.GetComponent<Renderer>().material = onMaterial;
        //Debug.Log("annotationtype: " + booleanSync.returnAnnotationType());
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

            // Get the current number of points in the Line Renderer
            int currentPoints = lineRenderer1.positionCount;

            // Increase the point count to accommodate the new point
            lineRenderer1.positionCount = currentPoints + 1;

            // Set the new point at the end of the Line Renderer
            lineRenderer1.SetPosition(currentPoints, annotationObject.transform.position);

            if (annotationTracker != null)
            {
                // Convert the LineRenderer world position to local space of the annotationTracker's parent
                Vector3 worldPosition = lineRenderer1.GetPosition(currentPoints);
                Vector3 localPosition = annotationTracker.transform.parent.InverseTransformPoint(worldPosition);

                // Assign the local position
                annotationTracker.transform.localPosition = localPosition;
            }
            else
            {
                Debug.LogError("Transform annotation tracker not properly connected to network");
                yield return null;
            }

            //used to instantiate start block for the start of an annotation
            if (startBlockBool == true)
            {
                GameObject startBlock = Instantiate(startEndBlock, annotationObject.transform.position, Quaternion.Euler(0f, 0f, 0f));
                startBlock.SetActive(true);
                startBlock.transform.SetParent(startEndHolder.transform);
                startBlock.GetComponent<Renderer>().material = startMaterial;
                startBlock.transform.localScale = new Vector3(0.006f, 0.006f, 0.005f);
                startBlockBool = false;
            }

            //used to instantiate arrow prefabs along line renderer everying 40 frames
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
                drewAtLeastOneArrow = true;
            }
        }
    }

    private void endBlock()
    {
        if (checkConnection() == false)
        {
            return;
        }

        //check to confirm that the index is within bounds
        if (parentHolderLineRenderer.transform.childCount - 1 < 0)
        {
            Debug.LogError("End block method executed accidentally even though there are no line renderer components. Bug detected");
        }

        //method used to access last linernedrer component in the linerendrerer dump
        Transform temp = parentHolderLineRenderer.transform.GetChild(parentHolderLineRenderer.transform.childCount -1);

        LineRenderer lastLine = temp.GetComponent<LineRenderer>();

        if (draw)
        {
            Debug1.text += "\nStopping to draw";
            Debug.Log("Stopping to draw");
            draw = false;
            //setting up the position, rotation of the end block
            Transform lastChild;
            if (parentHolderBall.transform.childCount - 1 <= 0 && drewAtLeastOneArrow == false)
            {
                lastChild = null;
            }
            else
            {

                lastChild = parentHolderBall.transform.GetChild(parentHolderBall.transform.childCount - 1);
            }
 
            GameObject endBlock = Instantiate(startEndBlock, annotationObject.transform.position, Quaternion.Euler(0f, 0f, 0f));
            endBlock.SetActive(true);
            endBlock.transform.SetParent(startEndHolder.transform);
            endBlock.GetComponent<Renderer>().material = endMaterial;
            endBlock.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);

            //there are three scenarios for instantiating the last block
            /*scenario one, for some odd reason, the user decided to dip their hand quickly in and out of the mesh, so the lindRend has no positions contained within the lineRenderer (i.e. no lines)
             * scenario two, the last block's position is the same as the last arrow prefab, so it replaces it
             * scenario three, the last block's position is different compared to the last arrow prefab, so it instantiates at the last position of the line renderer
             */
            if (lastLine.positionCount == 0)
            {
                //scenario 1
                endBlock.transform.position = lastLine.GetComponent<Transform>().position;
            }
            else if (lastChild.transform.position == lastLine.GetPosition(lastLine.positionCount - 1) && drewAtLeastOneArrow == false)
            {
                //scenario 2
                endBlock.transform.position = lastChild.transform.position;
                endBlock.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                Destroy(lastChild.gameObject);
            }
            else
            {
                //scenario 3
                endBlock.transform.position = lastLine.GetPosition(lastLine.positionCount - 1);
            }
            referenceSphere.GetComponent<Renderer>().material = offMaterial;
            if (FindObjectOfType<BooleanSync>() != null)
            {
                booleanSync = FindObjectOfType<BooleanSync>();

                booleanSync.setIsDrawing(false);
            }
            drewAtLeastOneArrow = false;
            StopAllCoroutines();
        }
    }

    //exporting coordinates to the csv
    void ExportCoordinatesToCSV()
    {
        //accessing annotation previous completed
        Transform lastChild = parentHolderLineRenderer.transform.GetChild(parentHolderLineRenderer.transform.childCount - 1);

        //use case scenario for when there are no actual annotations to export
        if (lastChild.GetComponent<LineRenderer>() == null)
        {
            Debug1.text += "\nAn error has occured exporting the CSV";
            return;
        }

        LineRenderer lineRenderer = lastChild.GetComponent<LineRenderer>();

        // Combine it with the filename you want to access
        string filePathAnnotation = Path.Combine(Application.persistentDataPath, "Annotation_coordinates_line_renderer.csv");

        // Open or create the CSV file
        using (StreamWriter writer = new StreamWriter(filePathAnnotation, true))
        {
            // Write header
            writer.WriteLine("start");
            //output annotation type
            /*
             * generalCorrection
             * Sutures
             * Incision
             * Excision
             * Insertion
             * Exploration
             */
            writer.WriteLine($"{annotationName}");

            // Loop through all the positions in the LineRenderer
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                // Get the position at index i
                Vector3 position = lineRenderer.GetPosition(i);

                // Write the index and coordinates to the CSV
                writer.WriteLine($"{i},{position.x},{position.y},{position.z}");
            }
            writer.WriteLine("end");
        }

        Debug.Log($"Coordinates exported to {filePathAnnotation}");
        Debug1.text += $"\nFile Path name: {filePathAnnotation}";
    }

    //insignficant function for writing debug statements
    public void addText(string placeHolder)
    {
        Debug1.text += $"\n{placeHolder}";
    }
    //function called to assign the annotation tracker once a network connection has been made
    public void setupAnnotationTracker(Transform transform)
    {
        annotationTracker = transform;
    }
    //method to check if properly connected to photon network
    private bool checkConnection()
    {
        if (FindObjectOfType<BooleanSync>() != null)
        {
            booleanSync = FindObjectOfType<BooleanSync>();
            if (booleanSync.returnIsConnected() == true)
            {
                return true;
            }
        }
        return false;
    }
}

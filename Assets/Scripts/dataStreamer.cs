using Microsoft.MixedReality.Toolkit.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class dataStreamer : MonoBehaviour
{
    // Path to your CSV file in the persistent data path
    private string filePathAnnotation;

    //debug window text used to display debug messages
    public TMP_Text Debug1;

    //gameobjects used to hold the annotations
    /*
     * parentBallholdr holds the arrows during gameplay
     * parentHolderLineRenderer holds the linerenderer components
     * startend holder holds the start and end points of the annotations
     */
    public GameObject parentHolderBall, parentHolderLineRenderer, startEndHolder;


    //annotationObject holds the arrow prefab that is instantiated along the linerenderer/annotations
    //startendblock holds the cube prefabs used to mark the start and end of an annotation
    public GameObject annotationObject, startEndBlock;

    //bool used to mark when an annotation is starting 
    private bool startBlockBool;

    //bool used to signify whether or not an annotation is being drawn
    [HideInInspector] protected bool draw;

    //materials used to colorcoat the annotations
    public Material startMaterial, endMaterial;

    //gameobject used to create multiple line renderers
    public GameObject lineRend;

    //int used to track when to instantiate arrows
    private int arrowCount;

    //reference to the line renderer being modified and updated
    private LineRenderer lineRenderer1;

    //List of line materials
    public List<Material> listMaterials = new List<Material> ();

    //////test button
    //public Interactable button;

    [HideInInspector] public Transform annotationTracker; 

    private BooleanSync booleanSync;

    private Queue<Func<IEnumerator>> transformTransport = new Queue<Func<IEnumerator>>();


    private bool isProcessing;

    private Vector3 lastPosition;

    void Start()
    {
        //setting default values for some variables
        startBlockBool = true;
        arrowCount = 0;
        lineRenderer1 = null;
        if (FindObjectOfType<BooleanSync>() != null)
        {
            booleanSync = FindObjectOfType<BooleanSync>();
            StartCoroutine(CheckNetworkConnectionCoroutine());

        }
        else
        {
            Debug.LogError("Error retrieving boolean drawing condition. Gameplay will be affected");
        }
        isProcessing = false;   
    }

    private IEnumerator ProcessQueueContinuously()
    {
        while (true) // Run indefinitely
        {
            if (!isProcessing && transformTransport.Count > 0)
            {
                isProcessing = true;

                // Dequeue and execute the next method
                Func<IEnumerator> nextMethod = transformTransport.Dequeue();
                yield return StartCoroutine(nextMethod());

                isProcessing = false;
            }

            // Yield to avoid blocking the main thread
            yield return null;
        }
    }
    private IEnumerator CheckPositionChanges()
    {
        while (true)
        {
            Debug.Log("Tracking annotation tracker positional changes");
            if (annotationTracker.position != lastPosition)
            {
                Debug.Log("Position changed!");
                // Execute the action when the boolean becomes true
                transformTransport.Enqueue(() => InstantiateCoroutine(annotationTracker.position));
                lastPosition = annotationTracker.position;
            }
            if (booleanSync.returnIsDrawing() == false)
            {
                yield return new WaitForSeconds(0.25f);
                StopCoroutine(ProcessQueueContinuously());
                endBlock();
                break;
            }
            // Check position every 0.1 seconds
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private IEnumerator CheckBoolRoutine()
    {

        while (!booleanSync.returnIsDrawing()) 
        { 
                
            Debug.Log("still stuck");
            yield return null; // Wait for the next frame
                
        }
        yield return new WaitForSeconds(0.3f);

        // setting up the line renderer
        GameObject temp1 = Instantiate(lineRend);
        lineRenderer1 = temp1.GetComponent<LineRenderer>();
        temp1.transform.SetParent(parentHolderLineRenderer.transform);

        //setting up annotation material
        switch (booleanSync.returnAnnotationType())
        {
            //output annotation type
            /*
             * generalCorrection
             * Sutures
             * Incision
             * Excision
             * Insertion
             * Exploration
             */
            case "generalcorrection":
                lineRenderer1.material = listMaterials[0];
                break;

            case "sutures":
                lineRenderer1.material = listMaterials[1];
                break;

            case "incision":
                lineRenderer1.material = listMaterials[2];
                break;

            case "excision":
                lineRenderer1.material = listMaterials[3];
                break;

            case "insertion":
                lineRenderer1.material = listMaterials[4];
                break;

            case "exploration":
                lineRenderer1.material = listMaterials[5];
                break;

            default:
                Debug.Log(booleanSync.returnAnnotationType() + ". here's the error");
                Debug.LogError("An error has occured");
                break;
        }

        // starting annotation systems
        Debug.Log("Annotation streaming commencing");
        StartCoroutine(ProcessQueueContinuously());
        StartCoroutine(CheckPositionChanges());
    }

    private IEnumerator CheckNetworkConnectionCoroutine()
    {
        Debug.Log("halo");
        Debug.Log("isConnected: " + booleanSync.returnIsConnected());

        // Wait until the network connection is established
        while (!booleanSync.returnIsConnected())
        {
            Debug.Log("Stuck in network connection");
            yield return new WaitForSeconds(0.25f); // Check every 0.5 seconds
        }

        Debug.Log("Network connected!");
        StartCoroutine(CheckBoolRoutine());
    }
    private IEnumerator InstantiateCoroutine(Vector3 newPosition)
    {
        if (lineRenderer1 == null)
        {
            string parentName = lineRenderer1.transform.parent.name;
            Debug.LogError("lineRenderer1 is not set! Make sure it is initialized before calling InstantiateCoroutine.");
            yield break;  // Stop the coroutine if lineRenderer1 is null
        }

        lineRenderer1.startWidth = 0.002f;
        lineRenderer1.endWidth = 0.002f;
        //shouldnt be needed for data streaming
        //Reset the frame count, so that the code only runs every 20 frames during gameplay
        int frameCount = 0;

        // Wait for # frames before instantiating the object
        while (frameCount < 20)
        {
            frameCount++;
            yield return null; // Wait for the next frame
        }

        Debug.Log("Instantiating annotation");

        // Adding Line Renderer component
        try
        {
            // Get the current number of points in the Line Renderer
            int currentPoints = lineRenderer1.positionCount;

            // Increase the point count to accommodate the new point
            lineRenderer1.positionCount = currentPoints + 1;

            // Set the new point at the end of the Line Renderer
            lineRenderer1.SetPosition(currentPoints, newPosition);
            //arrowCount is an int used to count and ensure that an arrow prefab is only instantiating once for every two annotation coordaintes
            arrowCount++;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        //used to instantiate start block for the start of an annotation
        if (startBlockBool == true)
        {
            GameObject startBlock = Instantiate(startEndBlock, newPosition, Quaternion.Euler(0f, 0f, 0f));
            startBlock.SetActive(true);
            startBlock.transform.SetParent(startEndHolder.transform);
            startBlock.GetComponent<Renderer>().material = startMaterial;
            startBlock.transform.localScale = new Vector3(0.006f, 0.006f, 0.005f);
            startBlockBool = false;
        }
        //used to instantiate arrow prefabs along line renderer everying 40 frames
        else if (arrowCount == 2)
        {
            //getting the vector based off the points of the previous two line renderers
            Vector3 direction = lineRenderer1.GetPosition(lineRenderer1.positionCount - 1) - lineRenderer1.GetPosition(lineRenderer1.positionCount - 2);

            //adjusting the direction of the arrow
            Quaternion quaternion = Quaternion.LookRotation(direction);
            Quaternion offset = Quaternion.Euler(0, 90, 0);
            quaternion = quaternion * offset;

            // Instantiate the annotation object at the parentHolder's position and rotation
            GameObject newAnnotation = Instantiate(annotationObject, newPosition, quaternion);
            newAnnotation.SetActive(true);
            newAnnotation.transform.SetParent(parentHolderBall.transform);
            newAnnotation.transform.localScale = new Vector3(0.3f, 0.3f, 0.6f);
            arrowCount = 0;
        }
        
        
    }

    private void endBlock()
    {
        //method used to access last linernedrer component in the linerendrerer dump
        Transform temp = parentHolderLineRenderer.transform.GetChild(parentHolderLineRenderer.transform.childCount - 1);
        LineRenderer lastLine = temp.GetComponent<LineRenderer>();
 
        Debug1.text += "\nStopping to draw";
        Debug.Log("Stopping to draw");
        //setting up the position, rotation of the end block
        Transform lastChild = parentHolderBall.transform.GetChild(parentHolderBall.transform.childCount - 1);
        GameObject endBlock = Instantiate(startEndBlock, annotationObject.transform.position, Quaternion.Euler(0f, 0f, 0f));
        endBlock.SetActive(true);
        endBlock.transform.SetParent(startEndHolder.transform);
        endBlock.GetComponent<Renderer>().material = endMaterial;
        endBlock.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);

        //there are two scenarios for instantiating the last block
        /* scenario one, the last block's position is the same as the last arrow prefab, so it replaces it
        * scenario two, the last block's position is different compared to the last arrow prefab, so it instantiates at the last position of the line renderer
        */

        if (lastChild.transform.position == lastLine.GetPosition(lastLine.positionCount - 1))
        {
            //scenario one
            endBlock.transform.position = lastChild.transform.position;
            endBlock.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            Destroy(lastChild.gameObject);
        }
        else
        {
            //scenario two
            //getting the vector based off the points of the previous two line renderers
            Vector3 direction = lastLine.GetPosition(lastLine.positionCount - 1) - lastLine.GetPosition(lastLine.positionCount - 2);

            endBlock.transform.position = lastLine.GetPosition(lastLine.positionCount - 1);
        }
        arrowCount = 0;
        startBlockBool = true;
        StartCoroutine(CheckBoolRoutine());
    }

    public void setupAnnotationTracker(Transform transform)
    {
        annotationTracker = transform;

        lastPosition = transform.position;
    }

}

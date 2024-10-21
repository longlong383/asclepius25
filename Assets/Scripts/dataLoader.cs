using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class dataLoader : MonoBehaviour
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
    [HideInInspector] public bool draw;

    //materials used to colorcoat the annotations
    public Material lineMaterial, startMaterial, endMaterial;

    //gameobject used to create multiple line renderers
    public GameObject lineRend;

    //int used to track when to instantiate arrows
    private int arrowCount;

    private LineRenderer lineRenderer1;
    void Start()
    {
        draw = false;
        startBlockBool = false;
        arrowCount = 0;
        try
        {
            filePathAnnotation = Path.Combine(Application.persistentDataPath, "Annotation_coordinates_line_renderer.csv");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error extracting file: " + ex.Message);
        }
        StartCoroutine(ReadCSV(filePathAnnotation));
    }

    private IEnumerator ReadCSV(string filePath)
    {
        lineRenderer1 = null;
        string[] row = null;

        // Use a try block to handle file reading, but keep yield returns outside
        try
        {
            Debug.Log("test1");
            // Read all lines from the CSV file
            row = File.ReadAllLines(filePath);  // Can throw exception
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading CSV: " + e.Message);
            yield break; // Exit the coroutine if there's an error
        }

        bool getInfo = false;
        Debug.Log("test2");

        for (int i = 0; i < row.Length; i++)
        {
            string temp = row[i];
            Debug.Log("\ntest3");

            if (temp == "start")
            {
                GameObject temp1 = Instantiate(lineRend);
                lineRenderer1 = temp1.GetComponent<LineRenderer>();
                temp1.transform.SetParent(parentHolderLineRenderer.transform);
                lineRenderer1.startWidth = 0.002f;
                lineRenderer1.endWidth = 0.002f;
                startBlockBool = true;
                arrowCount = 0;
                i++;
                getInfo = true;
                Debug.Log("\ngothere");
                continue;
            }
            if (temp == "end")
            {
                getInfo = false;
                lineRenderer1 = null;
                endBlock();
                Debug.Log("\nend");
                continue;
            }

            string[] values = temp.Split(',');

            if (IsNumeric(values[1]) && IsNumeric(values[2]) && IsNumeric(values[3]) && getInfo == true)
            {
                float x = float.Parse(values[1]);
                float y = float.Parse(values[2]);
                float z = float.Parse(values[3]);

                Vector3 point = new Vector3(x, y, z);
                Debug.Log($"\nX: {point.x} Y: {point.y} Z: {point.z}");

                // Move the coroutine call outside the try-catch block
                yield return StartCoroutine(InstantiateCoroutine(point));  // You can safely yield here
            }
        }
    }



    // Helper function to check if a string is numeric
    bool IsNumeric(string value)
    {
        return float.TryParse(value, out _);
    }

    private IEnumerator InstantiateCoroutine(Vector3 newPosition)
    {
        if (lineRenderer1 == null)
        {
            string parentName = lineRenderer1.transform.parent.name;
            Debug.Log("Parent GameObject's Name: " + parentName);
            Debug.LogError("lineRenderer1 is not set! Make sure it is initialized before calling InstantiateCoroutine.");
            yield break;  // Stop the coroutine if lineRenderer1 is null
        }
        else
        {
            string parentName = lineRenderer1.transform.parent.name;
            Debug.Log("Parent GameObject's Name: " + parentName);
        }
        //Reset the frame count
        int frameCount = 0;
        Debug.Log("hello" + newPosition);
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
            arrowCount++;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        //used to instantiate start block for the start of an annotation
        if (startBlockBool == true)
        {
            GameObject startBlock = Instantiate(startEndBlock, newPosition, Quaternion.Euler(0f,0f,0f));
            startBlock.SetActive(true);
            startBlock.transform.SetParent(startEndHolder.transform);
            startBlock.GetComponent<Renderer>().material = startMaterial;
            startBlock.transform.localScale = new Vector3(0.006f, 0.006f, 0.005f);
            startBlockBool = false;
        }
        //used to instantiate arrow prefabs along line renderer everying 40 frames
        else if (arrowCount == 2)
        {
            Debug.Log("test23");
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
        draw = false;
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

        StopAllCoroutines();
        
    }
}

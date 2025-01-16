using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

//this file is for controlling the actual annotations on unity scene
//functions like drawing the annotation and exporting the annotation details to a csv file
//exports csv to storage

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
    public List <Transform> thingsToReset = new List<Transform>();

    //original positions of the gameObjects at the start of gameplay from thingsToReset
    private List<Transform> storage = new List<Transform>();

    public Transform torso;

    [HideInInspector] public Transform annotationTracker;

    private BooleanSync booleanSync;

    void Start()
    {
        //setting up variables
        annotationName = null;
        draw = false;
        startBlockBool = false;

        annotationTracker = null;

        //accessing speech handlder
        CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this);
        Debug1.text += "\nStarting Annotation System";

        //Getting original positions of everything, and restore everything back to their original positions upon command
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
    
    //can also implement google drive api for more seamless uploads possibly.
   // Call this function when you want to trigger sharing
    public void ShareCSVFile(string filePath)
    {
        string csvFilePath = Path.Combine(Application.persistentDataPath, filePath);
        
        if (File.Exists(csvFilePath))
        {
            #if UNITY_ANDROID
            ShareToAndroid("Check out this CSV file!", csvFilePath);
            #elif UNITY_IOS
            iOSShare.ShareToiOS(csvFilePath);
            #else
            Debug.Log("Sharing is not supported on this platform.");
            Debug.Log($"Sharing file at: {filePath}");

            #endif
        }
        else
        {
            Debug.LogError("CSV file does not exist at the specified path.");
        }
    }

    // Android sharing method
    public static void ShareToAndroid(string message, string filePath)
    {
#if UNITY_ANDROID
        using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
        using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent"))
        {
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
            {
                AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + filePath);
                intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uri);
                intentObject.Call<AndroidJavaObject>("setType", "application/csv");
            }
            using (AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                activity.Call("startActivity", intentObject);
            }
        }
#endif
    }

    // iOS sharing method
    public class iOSShare
    {
        public static void ShareToiOS(string filePath)
        {
            #if UNITY_IOS
            string path = filePath;
            UnityEngine.iOS.Device.SetNoBackupFlag(path);
            // Implement iOS sharing code here using a native plugin or custom sharing method.
            #endif
        }
    }

    
    

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
            default:
                Debug.Log($"Unknown option {eventData.Command.Keyword}");
                break;
        }
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
        for (int i = 0; i < thingsToReset.Count; i++)
        {
            thingsToReset[i].position = storage[i].position;
            thingsToReset[i].rotation = storage[i].rotation;
            thingsToReset[i].localScale = storage[i].localScale;
        }
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
            Destroy(line.gameObject);
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

    //continuously running function for annotating
    //marks the line renders with arrows (which are now being kept out)
    //
    private IEnumerator InstantiateCoroutine()
    {
        if (FindObjectOfType<BooleanSync>() == null)
        {
            StopAllCoroutines();
        }

        booleanSync = FindObjectOfType<BooleanSync>();

        if (booleanSync.returnIsConnected() == false)
        {
            StopAllCoroutines();
        }

        Debug.Log("boolean status before: " + booleanSync.returnIsDrawing());
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
        Debug.Log("annotationtype: " + booleanSync.returnAnnotationType());
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

            if (annotationTracker != null)
            {
                annotationTracker.transform.position = annotationObject.transform.position;
            }
            else
            {
                Debug.LogError("Transform annotation tracker not properly connected to network");
                yield return null;
            }
            // Adding Line Renderer component

            // Get the current number of points in the Line Renderer
            int currentPoints = lineRenderer1.positionCount;

            // Increase the point count to accommodate the new point
            lineRenderer1.positionCount = currentPoints + 1;

            // Set the new point at the end of the Line Renderer
            lineRenderer1.SetPosition(currentPoints, annotationObject.transform.position);

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
            }
        }
    }

    private void endBlock()
    {
        if (checkConnection() == false)
        {
            return;
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
            referenceSphere.GetComponent<Renderer>().material = offMaterial;
            if (FindObjectOfType<BooleanSync>() != null)
            {
                booleanSync = FindObjectOfType<BooleanSync>();

                booleanSync.setIsDrawing(false);
            }
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

        //determine the file path for storage:
        string filePathAnnotation; 
            
        // Combine it with the filename you want to access
#if UNITY_ANDROID
    filePathAnnotation = Path.Combine("/storage/emulated/0/Download", "Annotation_coordinates_line_renderer.csv");
#elif UNITY_IOS
    // On iOS, files are saved in the app's sandbox; modify as needed for exporting later
    filePathAnnotation = Path.Combine(Application.persistentDataPath, "Annotation_coordinates_line_renderer.csv");
#else
        // Default to persistent data path for other platforms
        filePathAnnotation = Path.Combine(Application.persistentDataPath, "Annotation_coordinates_line_renderer.csv");
#endif
        
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
            //Header
            Debug1.text += $"\nAnnotation name: {annotationName}";

            writer.WriteLine($"Annotation Name: {annotationName}");
            writer.WriteLine("Index,Timestamp,X,Y,Z");

            // Call this from your Unity C# script
            // SHare("Check out this message!", "path/to/your/file.jpg");
            

            // Loop through all the positions in the LineRenderer
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                // Get the position at index i
                Vector3 position = lineRenderer.GetPosition(i);

                // Get the current timestamp
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                // Write the index, timestamp, and coordinates to the CSV
                writer.WriteLine($"{i},{timestamp},{position.x},{position.y},{position.z}");
            }
            writer.WriteLine("end");
        }

        //Share CSV File
        ShareCSVFile(filePathAnnotation);
        
        Debug.Log($"Coordinates exported to {filePathAnnotation}");
        Debug1.text += $"\nFile Path name: {filePathAnnotation}";
        Debug1.text += $"\nTimestamp now: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}";

    }
    
    

    //insignficant function for writing debug statements
    public void addText(string placeHolder)
    {
        Debug1.text += $"\n{placeHolder}";
    }

    public void setupAnnotationTracker(Transform transform)
    {
        annotationTracker = transform;
    }

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
using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeDetectionTraineeScene : MonoBehaviour
{
    // Start is called before the first frame update
    private BooleanSync booleanSync;
    private panicWarning panicWarning;
    protected bool arrows, startEndBlock;
    protected bool deletion;
    protected bool emergency;
    protected bool general;

    [SerializeField] private GameObject parentHolderBall;
    [SerializeField] private GameObject parentHolderLineRenderer;
    [SerializeField] private GameObject startEndHolder;

    void Start()
    {
        if (FindObjectOfType<BooleanSync>() && FindObjectOfType<panicWarning>())
        {
            booleanSync = FindObjectOfType<BooleanSync>();
            panicWarning = FindObjectOfType<panicWarning>();
        }
        else
        {
            Debug.Log("Cannot find BooleanSync in " + this.gameObject.name);
        }
        StartCoroutine(CheckNetworkConnectionCoroutine());
    }

    //continously checks photon network to see if any values have been modified
    private IEnumerator boolMonitoring()
    {
        while (true)
        {
            //check to see if there's a need to delete annotations
            if (booleanSync.returnDeletion() != deletion)
            {
                Debug.Log("deletion boolsync: " + booleanSync.returnDeletion());
                Debug.Log("returnDeletion before : " + deletion);
                destroyEverything();
                deletion = booleanSync.returnDeletion();
                Debug.Log("returnDeletion before : " + deletion);
            }

            //check to see if surgeon wants to make arrows disappear/reappear
            if (booleanSync.returnArrows() != arrows)
            {
                Debug.Log("arrows boolsync: " + booleanSync.returnArrows());
                Debug.Log("Arrows before: " + arrows);
                parentHolderBall.SetActive(booleanSync.returnArrows());
                arrows = booleanSync.returnArrows();
                Debug.Log("Arrows after " + arrows);
            }

            //make start/end cubes appear/disappear
            if (booleanSync.returnStartEndBlock() != startEndBlock)
            {
                Debug.Log("start end boolsync: " + booleanSync.returnStartEndBlock());
                Debug.Log("Startend before: " +  startEndBlock);
                startEndHolder.SetActive(booleanSync.returnStartEndBlock());
                startEndBlock = booleanSync.returnStartEndBlock();
                Debug.Log("Startend after: " + startEndBlock);
            }

            //emergency alert activation
            if (booleanSync.returnAlertEmergency() != emergency)
            {
                Debug.Log("alert emerge boolsync: " + booleanSync.returnAlertEmergency());
                Debug.Log("alert bool before: " + emergency);
                emergency = booleanSync.returnAlertEmergency();
                yield return panicWarning.StartCoroutine(panicWarning.ToggleObjectAndAudioWarning());
                Debug.Log("alert bool after: " + emergency);
                continue;
            } 

            //general alert activation
            if (booleanSync.returnAlertGeneral() != general)
            {
                Debug.Log("alert boolsync: " + booleanSync.returnAlertGeneral());
                Debug.Log("alert general before: " + emergency);
                general = booleanSync.returnAlertGeneral();
                yield return panicWarning.StartCoroutine(panicWarning.ToggleObjectAndAudioGeneral());
                Debug.Log("alert general before: " + emergency);
            }
            yield return null;
        }

    }

    //destroys all annotations objects in scene
    private void destroyEverything()
    {
        foreach (Transform child in parentHolderBall.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform line in parentHolderLineRenderer.transform)
        {
            if (line.name.Length >= 8 && line.name.ToLower().Substring(0,8) == "linerend")
            {
                Destroy(line.gameObject);
            }      
        }
        foreach (Transform block in startEndHolder.transform)
        {
            Destroy(block.gameObject);
        }
    }

    //method that continously checks to see if a network connection is made with photon network
    private IEnumerator CheckNetworkConnectionCoroutine()
    {
        // Wait until the network connection is established
        while (!booleanSync.returnIsConnected())
        {
            yield return new WaitForSeconds(0.25f); // Check every 0.5 seconds
        }

        //retrieving all bool values from the photon network
        arrows = booleanSync.returnArrows();
        startEndBlock = booleanSync.returnStartEndBlock();
        general = booleanSync.returnAlertGeneral();
        emergency = booleanSync.returnAlertEmergency();
        deletion = booleanSync.returnDeletion();
        StartCoroutine(boolMonitoring());
    }
}
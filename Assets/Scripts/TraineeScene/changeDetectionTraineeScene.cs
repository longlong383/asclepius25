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

    private IEnumerator boolMonitoring()
    {
        //yield return new WaitForSeconds(2f);
        while (true)
        {
            if (booleanSync.returnDeletion() != deletion)
            {
                Debug.Log("deletion boolsync: " + booleanSync.returnDeletion());
                //Debug.Log("alert bool before: " + booleanSync.returnAlertEmergency());
                //Debug.Log("alert general before: " + booleanSync.returnAlertGeneral());
                Debug.Log("returnDeletion before : " + deletion);
                destroyEverything();
                deletion = booleanSync.returnDeletion();
                Debug.Log("returnDeletion before : " + deletion);
            }
            if (booleanSync.returnArrows() != arrows)
            {
                Debug.Log("arrows boolsync: " + booleanSync.returnArrows());
                Debug.Log("Arrows before: " + arrows);
                parentHolderBall.SetActive(booleanSync.returnArrows());
                arrows = booleanSync.returnArrows();
                Debug.Log("Arrows after " + arrows);
            }

            if (booleanSync.returnStartEndBlock() != startEndBlock)
            {
                Debug.Log("start end boolsync: " + booleanSync.returnStartEndBlock());
                Debug.Log("Startend before: " +  startEndBlock);
                startEndHolder.SetActive(booleanSync.returnStartEndBlock());
                startEndBlock = booleanSync.returnStartEndBlock();
                Debug.Log("Startend after: " + startEndBlock);
            }

            if (booleanSync.returnAlertEmergency() != emergency)
            {
                Debug.Log("alert emerge boolsync: " + booleanSync.returnAlertEmergency());
                Debug.Log("alert bool before: " + emergency);
                emergency = booleanSync.returnAlertEmergency();
                yield return panicWarning.StartCoroutine(panicWarning.ToggleObjectAndAudioWarning());
                Debug.Log("alert bool after: " + emergency);
                continue;
            } 

            if (booleanSync.returnAlertGeneral() != general)
            {
                Debug.Log("alert boolsync: " + booleanSync.returnAlertGeneral());
                Debug.Log("alert general before: " + emergency);
                general = booleanSync.returnAlertGeneral();
                yield return panicWarning.StartCoroutine(panicWarning.ToggleObjectAndAudioGeneral());
                Debug.Log("alert general before: " + emergency);
            }
            yield return null;
            //yield return new WaitForSeconds(3f);
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

    private IEnumerator CheckNetworkConnectionCoroutine()
    {
        // Wait until the network connection is established
        while (!booleanSync.returnIsConnected())
        {
            yield return new WaitForSeconds(0.25f); // Check every 0.5 seconds
        }
        arrows = booleanSync.returnArrows();
        startEndBlock = booleanSync.returnStartEndBlock();
        general = booleanSync.returnAlertGeneral();
        emergency = booleanSync.returnAlertEmergency();
        deletion = booleanSync.returnDeletion();
        //yield return new WaitForSeconds(2f); 
        StartCoroutine(boolMonitoring());
    }
}
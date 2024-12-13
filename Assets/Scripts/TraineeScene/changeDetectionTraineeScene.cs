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
    protected bool deletion = false;
    protected bool emergency = false;
    protected bool general = false;

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
        while (true)
        {
            if (booleanSync.returnDeletion() != deletion)
            {
                //Debug.Log("alert bool before: " + booleanSync.returnAlertEmergency());
                //Debug.Log("alert general before: " + booleanSync.returnAlertGeneral());
                Debug.Log("returnDeletion before : " + booleanSync.returnDeletion());
                //destroyEverything();
                deletion = booleanSync.returnDeletion();
                //yield return new WaitForSeconds(0.1f);
                Debug.Log("returnDeletion before : " + booleanSync.returnDeletion());
            }
            if (booleanSync.returnArrows() != arrows)
            {
                Debug.Log("Arrows before: " + arrows);
                //parentHolderBall.SetActive(booleanSync.returnArrows());
                arrows = booleanSync.returnArrows();
                //yield return new WaitForSeconds(0.1f);
                Debug.Log("Arrows after " + arrows);
            }

            if (booleanSync.returnStartEndBlock() != startEndBlock)
            {
                Debug.Log("Startend before: " +  startEndBlock);
                //startEndHolder.SetActive(booleanSync.returnStartEndBlock());
                startEndBlock = booleanSync.returnStartEndBlock();
                //yield return new WaitForSeconds(0.1f);
                Debug.Log("Startend after: " + startEndBlock);
            }

            if (booleanSync.returnAlertEmergency() != emergency)
            {
                Debug.Log("alert bool before: " + booleanSync.returnAlertEmergency());
                //panicWarning.warning();
                emergency = booleanSync.returnAlertEmergency();
                //yield return new WaitForSeconds(0.1f);
                Debug.Log("alert bool after: " + booleanSync.returnAlertEmergency());
            }

            if (booleanSync.returnAlertGeneral() != general)
            {
                Debug.Log("alert general before: " + booleanSync.returnAlertGeneral());
                //panicWarning.annotation();
                general = booleanSync.returnAlertGeneral();
                //yield return new WaitForSeconds(0.1f);
                Debug.Log("alert general before: " + booleanSync.returnAlertGeneral());
            }

            yield return new WaitForSeconds(0.5f);
            //yield return null;
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
        arrows = !booleanSync.returnArrows();
        startEndBlock = !booleanSync.returnStartEndBlock();
        general = !booleanSync.returnAlertGeneral();
        emergency = !booleanSync.returnAlertEmergency();
        deletion = !booleanSync.returnDeletion();
        StartCoroutine(boolMonitoring());
        yield return null;
    }
}
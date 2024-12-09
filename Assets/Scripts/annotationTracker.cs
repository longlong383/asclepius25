using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class annotationTracker : MonoBehaviour
{
    [SerializeField] private GameObject annotationTrackerParentHolder;

    private int previousChildCount;


    private void Start()
    {
        if (annotationTrackerParentHolder == null)
        {
            Debug.LogError("Parent object is not assigned. Please assign it in the Inspector.");
            return;
        }

        // Store the initial child count of the parent GameObject
        previousChildCount = annotationTrackerParentHolder.transform.childCount;

        // Start checking for child addition
        StartCoroutine(CheckForChildAdded());
    }

    private IEnumerator CheckForChildAdded()
    {
        GameObject newChild = null;
        while (true)
        {
            if (annotationTrackerParentHolder.transform.childCount > previousChildCount)
            {
                // A child has been added
                newChild = FindNewChild();

                if (newChild != null)
                {
                    Debug.Log($"New child added! Name: {newChild.name}, Position: {newChild.transform.position}");
                }

                // Stop the coroutine after detecting the new child
                break;
            }

            // Wait for a short duration before checking again
            yield return new WaitForSeconds(0.1f);
        }

        newChild.GetComponent<Transform>().SetParent(annotationTrackerParentHolder.transform);
        // Ensure the position and rotation reset after re-parenting
        newChild.transform.localPosition = Vector3.zero;
        newChild.transform.localRotation = Quaternion.identity;

        if (FindObjectOfType<AnnotationController>() != null)
        {
            AnnotationController annotationController = FindObjectOfType<AnnotationController>();
            annotationController.setupAnnotationTracker(newChild.transform);
            Debug.Log("Tracker Setup");
        }

        if (FindObjectOfType<dataStreamer>() != null)
        {
            dataStreamer dataStreamer = FindObjectOfType<dataStreamer>();
            dataStreamer.setupAnnotationTracker(newChild.transform);
            Debug.Log("Tracker Setup");
        }

        yield break;
    }

    private GameObject FindNewChild()
    {
        // Look for the newly added child by comparing with the previous state
        foreach (Transform child in annotationTrackerParentHolder.transform)
        {
            if (child != null && child.GetSiblingIndex() >= previousChildCount && child.name == "Annotation Location Tracker(Clone)")
            {
                return child.gameObject;
            }
        }

        return null;
    }
}


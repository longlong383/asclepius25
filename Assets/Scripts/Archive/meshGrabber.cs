using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
//unused script, supposed to grab scene mesh information
public class meshGrabber : MonoBehaviour
{
    private List<IMixedRealitySpatialAwarenessMeshObserver> spatialObservers = new List<IMixedRealitySpatialAwarenessMeshObserver>();
    [SerializeField] private AnnotationController annotationController;

    void Start()
    {
        StartCoroutine(delayedStart());
    }

    private IEnumerator delayedStart()
    {
        yield return new WaitForSeconds(2f);
        // Check if the Spatial Awareness System is available
        if (CoreServices.SpatialAwarenessSystem != null)
        {
            IMixedRealityDataProviderAccess dataProviderAccess =
                CoreServices.SpatialAwarenessSystem as IMixedRealityDataProviderAccess;

            if (dataProviderAccess != null)
            {
                // Get all spatial awareness mesh observers and add them to the list
                IReadOnlyList<IMixedRealitySpatialAwarenessMeshObserver> observers = dataProviderAccess.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();
                int i = 0;
                foreach (IMixedRealitySpatialAwarenessMeshObserver observer in observers)
                {
                    // Add each observer to the list
                    spatialObservers.Add(observer);
                    annotationController.addText($"coordinates:{observer.ObserverOrigin}");
                    annotationController.addText($"size:{observer.ObservationExtents}");
                    annotationController.addText($"name: {observer.Name}");
                    // You can modify other properties of the observer if needed
                    observer.ObserverOrigin = new Vector3(-2f, 0f, 0f);  // Set to your desired coordinates
                    float placeHolder = 0.01f;
                    observer.ObservationExtents = new Vector3(placeHolder, placeHolder, placeHolder);  // Set to your desired volume dimensions
                    observer.UpdateInterval = 3f;  // Check every 3 seconds, adjust as needed
                    annotationController.addText($"{i} test");
                }

                annotationController.addText("Spatial observers configured and started.");
            }
            else
            {
                annotationController.addText("Failed to get data provider access.");
            }
        }
        else
        {
            annotationController.addText("Spatial Awareness System is null.");
        }
        foreach (IMixedRealitySpatialAwarenessMeshObserver observer in spatialObservers)
        {
            annotationController.addText($"coordinates:{observer.ObserverOrigin}");
            annotationController.addText($"size:{observer.ObservationExtents}");
            annotationController.addText($"name: {observer.Name}");
            observer.Enable();
            observer.Initialize();
            observer.Resume();
        }
    }
}



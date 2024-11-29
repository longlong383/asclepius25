using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collisionAnnotations : MonoBehaviour
{
    public AnnotationController annotationController;
    // Called when another collider enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        // Example: Do something when an object enters the trigger
        if (other.CompareTag("Phantom"))
        {
            // Add custom logic for when a player enters the trigger
            Debug.Log("Player has entered the trigger zone!");
            annotationController.StartDrawing();
        }
    }
    // Called when another collider exits the trigger
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Object exited the trigger: " + other.name);
        annotationController.StopDrawing();
    }
}

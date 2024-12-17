using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collisionAnnotations : MonoBehaviour
{
    public AnnotationController annotationController;

    // Called when annotation tracker collides with mesh of patient model
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("connection status: " + checkConnection());
        // Example: Do something when an object enters the trigger
        if (other.CompareTag("Phantom") && checkConnection() == true)
        {
            // Add custom logic for when a player enters the trigger
            Debug.Log("Player has entered the trigger zone!");
            annotationController.StartDrawing();
        }
    }

    // Called when annotation tracker exist the patient mesh model
    private IEnumerator OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Phantom") && checkConnection() == true)
        {
            Debug.Log("Object exited the trigger: " + other.name);
            yield return new WaitForSeconds(0.05f);
            annotationController.StopDrawing();
        }
        yield return null;
    }

    //check continously to see if a connection has been made with the photon network
    private bool checkConnection()
    {
        if (FindObjectOfType<BooleanSync>() != null)
        {
            BooleanSync booleanSync = FindObjectOfType<BooleanSync>();
            if (booleanSync.returnIsConnected() == true)
            {
                return true;
            }
        }
        return false;
    }
}

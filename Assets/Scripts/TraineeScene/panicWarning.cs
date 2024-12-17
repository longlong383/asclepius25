using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using UnityEngine;


public class panicWarning : MonoBehaviour
{
    public GameObject targetObject; // Assign your object in the Inspector
    public AudioSource audioSourceWarning; // Assign your AudioSource in the Inspector
    public AudioSource annotationIncoming;
    public Material warningMaterial, annotationMaterial;
    private int toggleCount = 5; // Number of times to toggle

    private float duration = 3f; // Total duration in seconds

    private BooleanSync booleanSync;

    private void Start()
    {
        if (FindObjectOfType<BooleanSync>() && FindObjectOfType<panicWarning>())
        {
            booleanSync = FindObjectOfType<BooleanSync>();
        }
        else
        {
            Debug.Log("Cannot find BooleanSync in " + this.gameObject.name);
        }
    }

    //emergency alert system activation upon command from changeDetection script
    public IEnumerator ToggleObjectAndAudioWarning()
    {
        targetObject.GetComponent<Renderer>().material = warningMaterial;
        for (int i = 0; i < toggleCount * 2; i++)
        {
            bool newState = !targetObject.activeSelf;
            targetObject.SetActive(newState); // Toggle the object

            audioSourceWarning.Play();
            yield return new WaitForSeconds(0.2f);
        }
        yield break;
    }

    //general alert system activation upon command from changeDetection script
    public IEnumerator ToggleObjectAndAudioGeneral()
    {
        float interval = duration / (toggleCount * 2);
        targetObject.GetComponent<Renderer>().material = annotationMaterial;
        for (int i = 0; i < 6; i++)
        {
            bool newState = !targetObject.activeSelf;
            targetObject.SetActive(newState); // Toggle the object

            annotationIncoming.Play();
            yield return new WaitForSeconds(0.4f);
        }
        yield break;
    }
}

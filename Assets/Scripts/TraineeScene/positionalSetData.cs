using UnityEngine;
using Photon.Pun;
using Microsoft.MixedReality.Toolkit.UI;


public class positionalSetData : MonoBehaviourPun
{
    [SerializeField] private Transform patient;
    [SerializeField] private Interactable reset, setView;
    [SerializeField] private Transform annotationDump;

    // Start is called before the first frame update

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    void Start()
    {
        initialPosition = patient.position;
        initialRotation = patient.rotation;
        initialScale = patient.localScale;

        setView.OnClick.AddListener(() => changeVisibility(setView.IsToggled, patient, annotationDump));
        reset.OnClick.AddListener(() => resetBody(reset.IsToggled));
    }

    private void resetBody(bool placeHolder)
    {
        patient.position = initialPosition;
        patient.rotation = initialRotation;
        patient.localScale = initialScale;
    }

    private void changeVisibility(bool placeHolder, Transform parent, Transform exception)
    {
        parent.GetComponent<MeshRenderer>().enabled = placeHolder;
        parent.GetComponent<ObjectManipulator>().enabled = placeHolder;
        foreach (Transform child in parent)
        {
            // Check if the child is the exception
            if (child != exception)
            {
                child.gameObject.SetActive(placeHolder); // Ensure the exception is active
            }
        }
    }
}

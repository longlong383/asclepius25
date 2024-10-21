using TMPro;
using UnityEngine;

public class DebugWindow : MonoBehaviour
{
    public TMP_Text textMesh;

    // Use this for initialization
    void Start()
    {
        textMesh = gameObject.GetComponent<TextMeshPro>();
    }

    void OnEnable()
    {
        Application.logMessageReceived += LogMessage;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogMessage;
    }

    public void LogMessage(string message, string stackTrace, LogType type)
    {
        if (textMesh.text.Length > 300)
        {
            textMesh.text = message + "\n";
        }
        else
        {
            textMesh.text += message + "\n";
        }
    }
}

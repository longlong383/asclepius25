using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;

public class BooleanSync : MonoBehaviourPunCallbacks
{
    protected const string BooleanKey = "isDrawing";
    protected const string BooleanKey1 = "isConnected";
    protected const string StringKey = "annotationType";

    // Global Hashtables with default values
    private Hashtable defaultProperties = new Hashtable
    {
        { BooleanKey, false },
        { BooleanKey1, false },
        { StringKey, "generalCorrection" }
    };

    public void setupEverything()
    {
        // Assign default values if connected to a room
        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(defaultProperties);
        }
    }

    // Set the boolean property
    public void setIsDrawing(bool value)
    {
        defaultProperties[BooleanKey] = value;
        if (PhotonNetwork.CurrentRoom != null)
        {
            Debug.Log("bool value being sent to isDrawing: " + value);
            Debug.Log("test21");
            PhotonNetwork.CurrentRoom.SetCustomProperties(defaultProperties);
            Debug.Log("Accesing directly fromd defaultProperties: " + defaultProperties[BooleanKey]);
        }
    }

    // Get the boolean property
    public bool returnIsDrawing()
    {
        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(BooleanKey, out var value))
        {
            Debug.Log("error check here: " + (bool)value);
            return (bool)value;
        }
        return (bool)defaultProperties[BooleanKey];
    }

    public void setIsConnected(bool value)
    {
        defaultProperties[BooleanKey1] = value;
        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(defaultProperties);
        }
    }

    // Get the boolean property
    public bool returnIsConnected()
    {
        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(BooleanKey1, out var value))
        {
            return (bool)value;
        }
        return (bool)defaultProperties[BooleanKey1];
    }

    public void setAnnotationType(string name)
    {
        defaultProperties[StringKey] = name;
        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(defaultProperties);
        }
    }

    public string returnAnnotationType()
    {
        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(StringKey, out var annotationType))
        {
            return (string)annotationType;
        }
        return (string)defaultProperties[StringKey];
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class STLImporter : MonoBehaviour
{
    public string stlFileName;
    public Material objectMaterial;

    // Start is called before the first frame update
    void Start()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, stlFileName);
        Mesh mesh = LoadSTL(filePath);

        if (mesh != null)
        {
            gameObject.SetActive(true); // Make sure the GameObject is active

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();

            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

            meshFilter.mesh = mesh;
            
           // Use the custom material if it has been assigned in the editor
            if (objectMaterial != null)
            {
                meshRenderer.material = objectMaterial;
            }
            else
            {
                // Fallback to a default material if no custom material is provided
                meshRenderer.material = new Material(Shader.Find("Standard"));
            }

            // Add or update a BoxCollider
            AddBoxCollider(gameObject, mesh);

            Debug.Log("Loaded STL file: " + stlFileName);
        }
        else
        {
            Debug.LogError("Failed to load STL file.");
        }
    }

    private void AddBoxCollider(GameObject gameObject, Mesh mesh)
    {
        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        if (boxCollider == null) boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = mesh.bounds.center;
        boxCollider.size = mesh.bounds.size;
    }

    // Update is called once per frame
    void Update()
    {

    }


    private Mesh LoadSTL(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("File not found: " + path);
            return null;
        }

        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    // Skip the header
                    br.ReadBytes(80);

                    // Read the number of triangles
                    uint triangleCount = br.ReadUInt32();

                    // print the triangle count
                    Debug.Log("Triangle count: " + triangleCount);

                    Vector3[] vertices = new Vector3[triangleCount * 3];
                    int[] triangles = new int[triangleCount * 3];

                    // Read triangles
                    for (int i = 0; i < triangleCount; i++)
                    {
                        // Skip normal
                        br.ReadBytes(12);

                        float baseScale = 0.01f;

                        // Read vertices
                        for (int j = 0; j < 3; j++)
                        {
                            float x = br.ReadSingle() * baseScale;
                            float y = br.ReadSingle() * baseScale;
                            float z = br.ReadSingle()* baseScale;
                            vertices[i * 3 + j] = new Vector3(x, y, z);
                        }

                        // Skip attribute byte count
                        br.ReadUInt16();

                        // Setup triangles
                        triangles[i * 3] = i * 3;
                        triangles[i * 3 + 1] = i * 3 + 1;
                        triangles[i * 3 + 2] = i * 3 + 2;
                    }

                    Mesh mesh = new Mesh();
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    mesh.vertices = vertices;
                    mesh.triangles = triangles;

                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();

                    return mesh;
                }
            }
        }
        catch (IOException e)
        {
            Debug.LogError("Error reading STL file: " + e.Message);
            return null;
        }
    }

}
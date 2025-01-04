using Microsoft.MixedReality.Toolkit.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataLoader2 : MonoBehaviour
{
    public TMP_Text Debug1;

    public GameObject parentHolderBall, parentHolderLineRenderer, startEndHolder;
    public GameObject annotationObject, startEndBlock, lineRend;
    public Material startMaterial, endMaterial;
    public List<Material> listMaterials = new List<Material>();

    private string filePathAnnotation;
    private LineRenderer lineRenderer1;

    private List<string[]> annotations = new List<string[]>(); // Stores annotations
    private int currentIndex = -1; // Tracks the current annotation index

    void Start()
    {
        filePathAnnotation = Path.Combine(Application.persistentDataPath, "Annotation_coordinates_line_renderer.csv");
        Debug1.text += "\n" + filePathAnnotation;
    }

    public void LoadAnnotations()
    {
        StartCoroutine(ReadCSV(filePathAnnotation));
    }

    private IEnumerator ReadCSV(string filePath)
    {
        lineRenderer1 = null;
        string[] rows;

        try
        {
            rows = File.ReadAllLines(filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading CSV: " + e.Message);
            yield break;
        }

        annotations.Clear();
        currentIndex = -1;

        for (int i = rows.Length - 1; i >= 0; i--) // Reverse iteration
        {
            annotations.Add(rows[i].Split(','));
        }

        if (annotations.Count > 0)
        {
            currentIndex = 0; // Start with the latest annotation
            LoadAnnotation(currentIndex);
        }
    }

    private void LoadAnnotation(int index)
    {
        if (index < 0 || index >= annotations.Count)
        {
            Debug1.text += "\nInvalid annotation index.";
            return;
        }

        string[] row = annotations[index];

        if (row[0].ToLower() == "start")
        {
            GameObject temp = Instantiate(lineRend);
            lineRenderer1 = temp.GetComponent<LineRenderer>();
            temp.transform.SetParent(parentHolderLineRenderer.transform);
            lineRenderer1.startWidth = 0.002f;
            lineRenderer1.endWidth = 0.002f;
        }
        else if (row[0].ToLower() == "end")
        {
            lineRenderer1 = null;
        }
        else if (lineRenderer1 != null)
        {
            float x = float.Parse(row[1]);
            float y = float.Parse(row[2]);
            float z = float.Parse(row[3]);
            Vector3 position = new Vector3(x, y, z);

            int pointCount = lineRenderer1.positionCount;
            lineRenderer1.positionCount = pointCount + 1;
            lineRenderer1.SetPosition(pointCount, position);
        }
    }

    public void NextAnnotation()
    {
        if (currentIndex + 1 < annotations.Count)
        {
            currentIndex++;
            LoadAnnotation(currentIndex);
        }
        else
        {
            Debug1.text += "\nNo more annotations.";
        }
    }

    public void PreviousAnnotation()
    {
        if (currentIndex - 1 >= 0)
        {
            currentIndex--;
            LoadAnnotation(currentIndex);
        }
        else
        {
            Debug1.text += "\nNo previous annotations.";
        }
    }
}

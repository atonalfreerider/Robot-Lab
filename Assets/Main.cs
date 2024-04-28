using System.Collections.Generic;
using System.IO;
using System.Linq;
using IxMilia.Step;
using IxMilia.Step.Schemas.ExplicitDraughting;
using UnityEngine;

public class Main : MonoBehaviour
{

    public string FolderPath;

    void Awake()
    {
        List<string> stepFilePaths = Directory.EnumerateFiles(FolderPath, "*.stp").ToList();

        List<FileInfo> stepFiles = stepFilePaths.Select(path => new FileInfo(path)).ToList();
        
        FileInfo largestFile = stepFiles.OrderByDescending(file => file.Length).First();
        
        StepFile stepFile = StepFile.Load(largestFile.FullName);

        foreach (StepItem stepItem in stepFile.Items)
        {
            switch (stepItem)
            {
                case StepLine stepLine:
                    LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer>();
                    Vector3 originPt = new Vector3((float)stepLine.Pnt.X, (float)stepLine.Pnt.Y, (float)stepLine.Pnt.Z);
                    Vector3 orientation = new Vector3((float) stepLine.Dir.Orientation.X, (float) stepLine.Dir.Orientation.Y, (float) stepLine.Dir.Orientation.Z);
                    Vector3 endPoint = originPt + orientation * (float)stepLine.Dir.Magnitude;
                    
                    lineRenderer.startWidth = 0.1f;
                    lineRenderer.endWidth = 0.1f;
                    lineRenderer.SetPositions(new[] {originPt, endPoint});
                    break;
                case StepCurve stepCurve:
                    break;
                case StepDirection stepDirection:
                    break;
                case StepCartesianPoint stepPoint:
                    Vector3 pt = new Vector3((float)stepPoint.X, (float)stepPoint.Y, (float)stepPoint.Z);
                    break;
                case StepPoint stepPoint:
                    break;
                case StepVector stepVector:
                    break;
                case StepGeometricRepresentationItem stepGeometricRepresentationItem:
                    break;
                case StepRepresentationItem stepRepresentationItem:
                    break;
            }
        }
    }
    
}
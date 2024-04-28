using System.IO;
using System.Linq;
using BasicLoader;
using Shapes;
using STPConverter;
using STPLoader;
using STPLoader.Implementation.Model;
using UnityEngine;

public class Main : MonoBehaviour
{
    public string stpPath;

    void Start()
    {
        foreach (string stpFilePath in Directory.EnumerateFiles(stpPath))
        {
            try
            {
                ILoader loader = LoaderFactory.CreateFileLoader(stpFilePath);
                IParser parser = ParserFactory.Create();
                IModel model = parser.Parse(loader);

                if (model is StpFile stpFile)
                {
                    IConverter converterFactory = ConverterFactory.Create();
                    MeshModel meshModel = converterFactory.Convert(stpFile);

                    Polygon polygon = PolygonFactory.Instance.PolygonPool.PolygonFromPool();
                    polygon.name = stpFile.Name;
                    Vector3[] points = new Vector3[meshModel.Points.Count];
                    int[] indices = meshModel.Triangles.ToArray();
                    int count = 0;
                    foreach (AForge.Math.Vector3 vector3 in meshModel.Points)
                    {
                        points[count] = new Vector3(vector3.X, vector3.Y, vector3.Z);
                        count++;
                    }

                    polygon.Draw3DPoly(points, Polygon.MirrorIndices(indices, 0));

                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        
    
    }
}
using BasicLoader;
using STPConverter;
using STPLoader;
using STPLoader.Implementation.Model;
using UnityEngine;

public class Main : MonoBehaviour
{
    public string stpPath;

    void Awake()
    {
        ILoader loader = LoaderFactory.CreateFileLoader(stpPath);
        IParser parser = ParserFactory.Create();
        IModel model = parser.Parse(loader);

        if (model is StpFile stpFile)
        {
            IConverter converterFactory = ConverterFactory.Create();
            MeshModel meshModel = converterFactory.Convert(stpFile);

            foreach (AForge.Math.Vector3 vector3 in meshModel.Points)
            {
                
            }
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BasicLoader;
using Shapes;
using STPLoader;
using STPLoader.Implementation.Model;
using STPLoader.Implementation.Model.Entity;
using UnityEngine;
using Circle = STPLoader.Implementation.Model.Entity.Circle;
using Plane = STPLoader.Implementation.Model.Entity.Plane;

public class Main : MonoBehaviour
{
    public string stpPath;

    void Start()
    {
        string stpFilePath = stpPath;
        ILoader loader = LoaderFactory.CreateFileLoader(stpFilePath);
        IParser parser = ParserFactory.Create();
        IModel model = parser.Parse(loader);

        if (model is StpFile stpFile)
        {
            Dictionary<long, Entity> data = stpFile.Data.All().ToDictionary(x => x.Key, x => x.Value);
            foreach ((long id, Entity entity) in data)
            {
                switch (entity)
                {
                    case AdvancedFace advancedFace:
                        break;
                    case Axis2Placement3D axis2Placement3D:
                        break;
                    case FaceBound faceBound:
                        break;
                    case FaceOuterBound faceOuterBound:
                        break;
                    case Bound bound:
                        break;
                    case BSplineCurveWithKnots bSplineCurveWithKnots:
                        break;
                    case DirectionPoint directionPoint:
                        break;
                    case CartesianPoint cartesianPoint:
                        break;
                    case Circle circle:
                        Shapes.Circle circleShape =
                            PolygonFactory.NewCirclePoly(PolygonFactory.Instance.mainMat);
                        circleShape.gameObject.SetActive(true);
                        circleShape.transform.SetParent(transform, false);
                        circleShape.DrawCirc((float)circle.Radius, 1, 0);
                        Axis2Placement3D circleAxis2Placement3D = data[circle.PointId] as Axis2Placement3D;
                        CartesianPoint circlePt = data[circleAxis2Placement3D.PointIds[0]] as CartesianPoint;
                        DirectionPoint dir1 = data[circleAxis2Placement3D.PointIds[1]] as DirectionPoint;
                        DirectionPoint dir2 = data[circleAxis2Placement3D.PointIds[2]] as DirectionPoint;
                        Vector3 circPos = new Vector3(circlePt.Vector.X, circlePt.Vector.Y, circlePt.Vector.Z);
                        circleShape.transform.localPosition = circPos;
                        circleShape.transform.localRotation = Quaternion.LookRotation(
                            new Vector3(dir1.Vector.X, dir1.Vector.Y, dir1.Vector.Z),
                            new Vector3(dir2.Vector.X, dir2.Vector.Y, dir2.Vector.Z));
                        break;
                    case ClosedShell closedShell:
                        break;
                    case ConicalSurface conicalSurface:
                        break;
                    case CylindricalSurface cylindricalSurface:
                        break;
                    case EdgeCurve edgeCurve:
                        break;
                    case EdgeLoop edgeLoop:
                        break;
                    case Line line:
                        Shapes.Lines.StaticLink staticLink =
                            Instantiate(Shapes.Lines.StaticLink.prototypeStaticLink);
                        staticLink.gameObject.SetActive(true);
                        staticLink.transform.SetParent(transform, false);

                        CartesianPoint linePt1 = data[line.Point1Id] as CartesianPoint;
                        VectorPoint lineVecPt = data[line.Point2Id] as VectorPoint;
                        DirectionPoint lineDirPt = data[lineVecPt.PointId] as DirectionPoint;
                        Vector3 directionVector = new Vector3(lineDirPt.Vector.X, lineDirPt.Vector.Y, lineDirPt.Vector.Z);
                        Vector3 lineStart =  new Vector3(linePt1.Vector.X, linePt1.Vector.Y, linePt1.Vector.Z);
                        Vector3 lineEnd = lineStart + directionVector * (float)lineVecPt.Length;

                        staticLink.LW = .1f;
                        staticLink.DrawFromTo(lineStart, lineEnd);
                        staticLink.SetColor(Color.magenta);
                        break;
                    case OrientedEdge orientedEdge:
                        break;
                    case Plane plane:
                        break;
                    case ToroidalSurface toroidalSurface:
                        break;
                    case Surface surface:
                        break;
                    case VectorPoint vectorPoint:
                        break;
                    case VertexPoint vertexPoint:
                        break;
                }
            }
        }
    }
}
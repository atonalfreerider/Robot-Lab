#nullable enable
using System.Collections.Generic;
using System.Linq;
using BasicLoader.Interface;
using Shapes;
using STPLoader.Implementation.Converter.Entity;
using STPLoader.Implementation.Model;
using STPLoader.Implementation.Model.Entity;
using STPLoader.Interface;
using Triangulation;
using UnityEngine;
using Circle = STPLoader.Implementation.Model.Entity.Circle;
using Plane = STPLoader.Implementation.Model.Entity.Plane;
using Vector3 = UnityEngine.Vector3;

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
            List<Entity> topLevelEntities = TopLevelEntities(data);

            foreach (Entity entity in topLevelEntities)
            {
                IConvertable? convertedEntity = CreateConvertable(entity, stpFile);

                switch (convertedEntity)
                {
                    case AdvancedFaceConvertable advancedFaceConvertable:
                        
                        break;
                    case Axis2Placement3DConvertable axis2Placement3DConvertable:
                        GameObject axisPt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        axisPt.name = $"{axis2Placement3DConvertable.GetType()}:{entity.Id}";
                        axisPt.transform.SetParent(transform, false);
                        axisPt.transform.localScale = new Vector3(.1f, .1f, .1f);
                        axisPt.transform.localPosition = new Vector3(axis2Placement3DConvertable.Location.X, axis2Placement3DConvertable.Location.Y, axis2Placement3DConvertable.Location.Z);
                        break;
                    case BoundConvertable boundConvertable:
                        break;
                    case CircleConvertable circleConvertable:
                        Shapes.Circle circleShape = DrawCircle(circleConvertable.Radius);
                        circleShape.name = $"{circleConvertable.GetType()}:{entity.Id}";
                        AForge.Math.Vector3 circLocation = circleConvertable.Axis2Placement3DConvertable.Location;
                        float yaw = circleConvertable.Yaw;
                        float pitch = circleConvertable.Pitch;
                        circleShape.transform.localPosition = new Vector3(circLocation.X, circLocation.Y, circLocation.Z);

                        circleShape.transform.localRotation = Quaternion.LookRotation(new Vector3(yaw, pitch, 0));
                        break;
                    case ClosedShellConveratable closedShellConveratable:
                        break;
                    case CylindricalSurfaceConvertable cylindricalSurfaceConvertable:
                        Shapes.Circle cylinderShape = DrawCylinder(cylindricalSurfaceConvertable.Radius, cylindricalSurfaceConvertable.Length);
                        cylinderShape.name = $"{cylindricalSurfaceConvertable.GetType()}:{entity.Id}";
                        AForge.Math.Vector3 cylinderLocation = cylindricalSurfaceConvertable.Axis2Placement3DConvertable.Location;
                        float yawCylinder = cylindricalSurfaceConvertable.Yaw;
                        float pitchCylinder = cylindricalSurfaceConvertable.Pitch;
                        cylinderShape.transform.localPosition = new Vector3(cylinderLocation.X, cylinderLocation.Y, cylinderLocation.Z);
                        cylinderShape.transform.localRotation = Quaternion.LookRotation(new Vector3(yawCylinder, pitchCylinder, 0));
                        break;
                    case EdgeCurveConvertable edgeCurveConvertable:
                        break;
                    case EdgeLoopConvertable edgeLoopConvertable:
                        Polygon edgeLoopPoly = PolygonFactory.NewPoly(PolygonFactory.Instance.mainMat);
                        edgeLoopPoly.name = $"{edgeLoopConvertable.GetType()}:{entity.Id}";
                        edgeLoopPoly.gameObject.SetActive(true);
                        edgeLoopPoly.transform.SetParent(transform, false);
                        
                        Vector3[] edgeLoopPoints = edgeLoopConvertable.Points.Select(p => new Vector3(p.X, p.Y, p.Z)).ToArray();
                        int[] edgeLoopIndices = edgeLoopConvertable.Indices.ToArray();
                        edgeLoopPoly.Draw3DPoly(edgeLoopPoints, Polygon.MirrorIndices(edgeLoopIndices, 0));
                        
                        break;
                    case FaceBoundConvertable faceBoundConvertable:
                        EdgeLoopConvertable edgeLoopConvertable2 = faceBoundConvertable.EdgeLoopConvertable;
                        Polygon edgeLoopPoly2 = MakeEdge(edgeLoopConvertable2);
                        edgeLoopPoly2.name = $"{edgeLoopConvertable2.GetType()}:{entity.Id}";
                        edgeLoopPoly2.gameObject.SetActive(true);
                        edgeLoopPoly2.transform.SetParent(transform, false);
                        
                        break;
                    case FaceOuterBoundConvertable faceOuterBoundConvertable:
                        break;
                    case LineConvertable lineConvertable:
                        Vector3 startPos = new Vector3(lineConvertable.Start.X, lineConvertable.Start.Y, lineConvertable.Start.Z);
                        Vector3 endPos = new Vector3(lineConvertable.End.X, lineConvertable.End.Y, lineConvertable.End.Z);
                        Shapes.Lines.StaticLink line = DrawLine(startPos, endPos);
                        line.name = $"{lineConvertable.GetType()}:{entity.Id}";
                        break;
                    case OrientedEdgeConvertable orientedEdgeConvertable:
                        break;
                    case PlaneConvertable planeConvertable:
                        break;
                    case SurfaceConvertable surfaceConvertable:
                        break;
                    case null:
                        switch (entity)
                        {
                            case CartesianPoint cartesianPoint:
                                GameObject pt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                                pt.name = $"{cartesianPoint.GetType()}:{entity.Id}";
                                pt.transform.SetParent(transform, false);
                                pt.transform.localScale = new Vector3(.1f, .1f, .1f);
                                pt.transform.localPosition = new Vector3(cartesianPoint.Vector.X, cartesianPoint.Vector.Y, cartesianPoint.Vector.Z);
                                break;
                        }
                        break;
                }
            }
        }
    }

    static List<Entity> TopLevelEntities(Dictionary<long, Entity> data)
    {
        List<long> referencedIds = new List<long>();
        foreach (Entity entity in data.Values)
        {
            foreach (string s in entity.Data)
            {
                if (s[0] == '#')
                {
                    long parsedId = long.Parse(s[1..]);
                    referencedIds.Add(parsedId);
                }
            }
        }

        return data.Values.Where(entity => !referencedIds.Contains(entity.Id)).ToList();
    }

    static IConvertable? CreateConvertable(Entity entity, IStpModel model)
    {
        switch (entity)
        {
            case AdvancedFace advancedFace:
                return new AdvancedFaceConvertable(advancedFace, model);
            case Axis2Placement3D axis2Placement3D:
                return new Axis2Placement3DConvertable(axis2Placement3D, model);
            case FaceBound faceBound:
                return new FaceBoundConvertable(faceBound, model);
            case FaceOuterBound faceOuterBound:
                return new FaceOuterBoundConvertable(faceOuterBound, model);
            case Bound bound:
                return new BoundConvertable(bound, model);
            case BSplineCurveWithKnots bSplineCurveWithKnots:
                break;
            case DirectionPoint directionPoint:
                break;
            case CartesianPoint cartesianPoint:
                break;
            case Circle circle:
                return new CircleConvertable(circle, model);
            case ClosedShell closedShell:
                return new ClosedShellConveratable(closedShell, model);
            case ConicalSurface conicalSurface:
                break;
            case CylindricalSurface cylindricalSurface:
                return new CylindricalSurfaceConvertable(cylindricalSurface, model);
            case EdgeCurve edgeCurve:
                return new EdgeCurveConvertable(edgeCurve, model);
            case EdgeLoop edgeLoop:
                return new EdgeLoopConvertable(edgeLoop, model);
            case Line line:
                return new LineConvertable(line, model);
            case OrientedEdge orientedEdge:
                return new OrientedEdgeConvertable(orientedEdge, model);
            case Plane plane:
                return new PlaneConvertable(plane, model);
            case ToroidalSurface toroidalSurface:
                break;
            case Surface surface:
                return new SurfaceConvertable(surface, model);
            case VectorPoint vectorPoint:
                break;
            case VertexPoint vertexPoint:
                break;
        }

        return null;
    }

    Shapes.Circle DrawCircle(float r)
    {
        Shapes.Circle circleShape =
            PolygonFactory.NewCirclePoly(PolygonFactory.Instance.mainMat);
        circleShape.gameObject.SetActive(false);
        circleShape.transform.SetParent(transform, false);
        circleShape.DrawCirc((float)r, 1, 0);

        return circleShape;
    }

    Shapes.Circle DrawCylinder(float r, float h)
    {
        Shapes.Circle cylinder = PolygonFactory.NewCirclePoly(PolygonFactory.Instance.mainMat);
        cylinder.gameObject.SetActive(true);
        cylinder.transform.SetParent(transform, false);


        cylinder.DrawCirc(r, 1, h);

        return cylinder;
    }

    Shapes.Lines.StaticLink DrawLine(Vector3 lineStart, Vector3 lineEnd)
    {
        Shapes.Lines.StaticLink staticLink =
            Instantiate(Shapes.Lines.StaticLink.prototypeStaticLink);
        staticLink.gameObject.SetActive(true);
        staticLink.transform.SetParent(transform, false);


        staticLink.LW = .1f;
        staticLink.DrawFromTo(lineStart, lineEnd);
        staticLink.SetColor(Color.magenta);

        return staticLink;
    }
    
    Polygon MakeEdge(EdgeLoopConvertable edgeLoopConvertable)
    {
        Polygon edgeLoopPoly = PolygonFactory.NewPoly(PolygonFactory.Instance.mainMat);
        edgeLoopPoly.gameObject.SetActive(true);
        edgeLoopPoly.transform.SetParent(transform, false);
        
        List<Vector2> edgeLoopPoints = edgeLoopConvertable.Points.Select(p => new Vector2(p.X, p.Y)).ToList();
        DelaunayTriangulator delaunayTriangulator = new DelaunayTriangulator(edgeLoopPoints);
        delaunayTriangulator.Triangulate();
        List<Triangle2D> triangle2Ds = delaunayTriangulator.GetTriangles();
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        
        foreach (Triangle2D triangle2D in triangle2Ds)
        {
            vertices.Add(triangle2D.a);
            vertices.Add(triangle2D.b);
            vertices.Add(triangle2D.c);
            
            indices.Add(vertices.Count - 3);
            indices.Add(vertices.Count - 2);
            indices.Add(vertices.Count - 1);
        }
        
        edgeLoopPoly.Draw3DPoly(vertices.ToArray(), Polygon.MirrorIndices(indices.ToArray(), 0));
        
        return edgeLoopPoly;
    }
}
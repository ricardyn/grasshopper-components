using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Linq;
using Rhino.Geometry.Intersect;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_d6b02 : GH_ScriptInstance
{
  #region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { /* Implementation hidden. */ }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { /* Implementation hidden. */ }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { /* Implementation hidden. */ }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
  #endregion

  #region Members
  /// <summary>Gets the current Rhino document.</summary>
  private readonly RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private readonly GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private readonly IGH_Component Component;
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private readonly int Iteration;
  #endregion
  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments,
  /// Output parameters as ref arguments. You don't have to assign output parameters,
  /// they will have a default value.
  /// </summary>
  #region Runscript
  private void RunScript(Surface externalRegion, List<Rectangle3d> roomRectangles, ref object penalty)
  {
    bool externalRegionIntersect =
      roomRectangles
      .Any(rec => DoesIntersect(GetRectangleSurface(rec), externalRegion));

    if (externalRegionIntersect)
    {
      penalty = true;
      Print("Some rectangle intersect external region");
      return;
    }

    bool intersectWithOther =
      roomRectangles
      .Any(rec =>
        IntersectWithOthers(
          rec,
          roomRectangles.Except(new List<Rectangle3d>() { rec })
        )
      );

    if (intersectWithOther)
    {
      penalty = true;
      Print("Some rectangle intersect other");
      return;
    }

    Print("There is no penalty");
    penalty = false;
  }
  #endregion
  #region Additional
  private bool IntersectWithOthers(Rectangle3d rectangle, IEnumerable<Rectangle3d> others)
  {
    return
      others
      .Any(other =>
        DoesIntersect(GetRectangleSurface(other), GetRectangleSurface(rectangle))
      );
  }

  private Surface GetRectangleSurface(Rectangle3d rectangle)
  {
    // Get rectangle segments as NURBS curves
    IEnumerable<NurbsCurve> segments =
      rectangle
      .ToPolyline()
      .GetSegments()
      .Select(s => s.ToNurbsCurve());

    var recSurface =
      Brep
      .CreatePatch(segments, null, 0.00001)
      .Surfaces
      .FirstOrDefault();

    return recSurface;
  }

  private bool DoesIntersect(Surface srfA, Surface srfB)
  {
    // out parameters to Intersection.SurfaceSurface() method
    var interCurves = new Curve[] { };
    var interPoints = new Point3d[] { };

    // checking intersections of rectangle surface with external region
    bool doesIntersect =
      Intersection
      .SurfaceSurface(
        srfA,
        srfB,
        0,
        out interCurves,
        out interPoints
      );

    return doesIntersect;
  }
  #endregion
}
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Capitalino.Core.Coordinates
{
    public static class CoordinConvert
    {
        public static Point3d TransformByTarget(this Point3d origin)
        {
            var target = (Point3d)Application.GetSystemVariable("TARGET");
            var readPt = new Point3d(
                origin.X-target.X,
                origin.Y-target.Y,
                origin.Z-target.Z);
            return readPt;
        }
        public static Point2d TransformByTarget(this Point2d origin)
        {
            var target = (Point3d)Application.GetSystemVariable("TARGET");
            var readPt = new Point2d(
                origin.X - target.X,
                origin.Y - target.Y);
            return readPt;
        }
        public static Extents3d TransformByTarget(this Extents3d origin)
        {
            return new Extents3d(origin.MinPoint.TransformByTarget(), origin.MaxPoint.TransformByTarget());
        }
        public static Extents2d TransformByTarget(this Extents2d origin)
        {
            return new Extents2d(origin.MinPoint.TransformByTarget(), origin.MaxPoint.TransformByTarget());
        }
        public static Extents2d ReduceDimensions(this Extents3d origin)
        {
            var max = origin.MaxPoint;
            var min = origin.MinPoint;
            return new Extents2d(min.X, min.Y, max.X, max.Y);
        }
    }
}
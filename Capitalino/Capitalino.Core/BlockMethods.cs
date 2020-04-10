using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Linq;

namespace Capitalino.Core
{
    public static class BlockMethods
    {
        public static void QuickBlock(this SelectionSet set, Point3d point, out BlockTableRecord btr, out BlockReference br)
        {
            var ents = from id in set.GetObjectIds()
                       let ent = id.GetObject(OpenMode.ForRead) as Entity
                       let layer = ent.LayerId.GetObject(OpenMode.ForRead) as LayerTableRecord
                       where !layer.IsLocked
                       select ent;

            if (ents.Count() == 0) throw new Exception("No invalid selected item.");

            var db = set.Database();
            var name = $"Z{DateTimeOffset.Now.ToUnixTimeMilliseconds()}";
            var bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
            if (bt.Has(name)) throw new Exception("Name used.");

            var msr = bt[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForWrite) as BlockTableRecord;
            btr = new BlockTableRecord { Name = name };

            var vt_toOrigin = point.GetVectorTo(new Point3d(0, 0, 0));
            var mt_toOrigin = Matrix3d.Displacement(vt_toOrigin);
            var mt_keepStatic = Matrix3d.Displacement(new Vector3d(0, 0, 0));

            foreach (var ent in ents)
            {
                //移动和复制分开，不然标注位置不会移动！
                var duplicatedEnt = ent.GetTransformedCopy(mt_keepStatic);
                duplicatedEnt.TransformBy(mt_toOrigin);
                btr.AppendEntity(duplicatedEnt);
                ent.UpgradeOpen();
                ent.Erase();
                ent.DowngradeOpen();
            }

            bt.UpgradeOpen();
            bt.Add(btr);

            var space = bt[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForWrite) as BlockTableRecord;
            br = new BlockReference(point, btr.Id);
            space.AppendEntity(br);
            space.DowngradeOpen();
        }
        public static void MassiveSuperExplode(this SelectionSet set, out DBObjectCollection col)
        {
            var ids = set.GetObjectIds();
            if (ids == null) throw new Exception("Nothing Selected.");

            var bfs = from id in ids
                      let ent = id.GetObject(OpenMode.ForRead) as Entity
                      let lyr = ent.LayerId.GetObject(OpenMode.ForRead) as LayerTableRecord
                      where !lyr.IsLocked && ent is BlockReference
                      select ent as BlockReference;

            var db = set.Database();
            var bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
            var modelspace = bt[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForRead) as BlockTableRecord;
            col = new DBObjectCollection();

            modelspace.UpgradeOpen();
            foreach (var bf in bfs)
            {
                foreach (var explodedObj in SuperExplode(bf, modelspace))
                {
                    col.Add(explodedObj as DBObject);
                }
            }
            modelspace.DowngradeOpen();
        }
        private static DBObjectCollection SuperExplode(BlockReference bf, BlockTableRecord modelspace)
        {
            var result = new DBObjectCollection();
            bf.UpgradeOpen();
            var objSet = new DBObjectCollection();
            bf.Explode(objSet);
            foreach (var obj in objSet)
            {
                var id = modelspace.AppendEntity(obj as Entity);
                var ent = id.GetObject(OpenMode.ForRead);
                if (ent is BlockReference)
                {
                    ent.UpgradeOpen();
                    var col = SuperExplode(obj as BlockReference, modelspace);
                    foreach (var explodedObj in col) result.Add(explodedObj as DBObject);
                }
                else
                {
                    result.Add(obj as Entity);
                }
            }
            bf.Erase();
            return result;
        }
    }
}
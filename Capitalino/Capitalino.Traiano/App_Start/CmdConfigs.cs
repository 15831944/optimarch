using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Capitalino.Traiano.Block;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

[assembly: CommandClass(typeof(Capitalino.Traiano.CmdConfigs))]
namespace Capitalino.Traiano
{
    public class CmdConfigs
    {
        [CommandMethod("ZPRINTSINGLE")]
        public static void ZPRINTSINGLE() => PublicationCmds.PrintSingle();
        [CommandMethod("ZPRINTMULTI")]
        public static void ZPRINTMULTI() => PublicationCmds.PrintMulti();
        [CommandMethod("ZCOUNTBLOCK")]
        public static void ZCOUNTBLOCK() => BlockCmds.CountBlockInstance();
        [CommandMethod("ZSELECT")]
        public static void ZSELECT()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            using(var trans = doc.TransactionManager.StartOpenCloseTransaction())
            {
                var setRs = doc.Editor.GetSelection();
                if (setRs.Status != PromptStatus.OK) return;
                var circles = from id in setRs.Value.GetObjectIds()
                              let obj = trans.GetObject(id, OpenMode.ForRead) as Circle
                              where obj != null
                              select obj;

                var items = new List<ObjectId>();

                foreach (var c in circles)
                {
                    var txt = from id in setRs.Value.GetObjectIds()
                              let ent = trans.GetObject(id, OpenMode.ForRead) as DBText
                              where ent != null
                              where (c.Bounds.Value.MinPoint.X < ent.Position.X
                              && ent.Position.X < c.Bounds.Value.MaxPoint.X
                              && c.Bounds.Value.MinPoint.Y < ent.Position.Y
                              && ent.Position.Y < c.Bounds.Value.MaxPoint.Y)
                              || ent.TextString.ToLower() == "a"
                              || ent.TextString.ToLower() == "b"
                              || ent.TextString.ToLower() == "c"

                              select id;
                    items.AddRange(txt);
                } 
                doc.Editor.SetImpliedSelection(items.ToArray());

            }
        }
    }
}
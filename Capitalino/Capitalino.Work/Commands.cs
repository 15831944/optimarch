using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Capitalino.Core;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

[assembly: CommandClass(typeof(Capitalino.Work.Commands))]
namespace Capitalino.Work
{
    public static partial class Commands
    {
        [CommandMethod("ZBLOCK", CommandFlags.UsePickSet)]
        public static void ZBLOCK()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            try
            {
                var setRs = doc.Editor.SelectImpliedOrGetSelection();
                if (setRs.Status != PromptStatus.OK) return;
                var resultPoint = doc.Editor.GetPoint("\nPick Insert Point:");
                if (resultPoint.Status != PromptStatus.OK) return;
                var point = resultPoint.Value;
                using (var trans = doc.TransactionManager.StartTransaction())
                {
                    setRs.Value.QuickBlock(point, out BlockTableRecord btr, out BlockReference br);
                    trans.AddNewlyCreatedDBObject(btr, true);
                    trans.AddNewlyCreatedDBObject(br, true);
                    trans.Commit();
                    doc.Editor.WriteMessage($"\nBlock: {btr.Name} Created!");
                }
            }
            catch (Exception ex)
            {
                doc.Editor.WriteMessage($"/n{ex.Message}");
            }
        }
        [CommandMethod("ZEXPLODE", CommandFlags.UsePickSet)]
        public static void ZEXPLODE()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            try
            {
                var setRs = doc.Editor.SelectImpliedOrGetSelection();
                if (setRs.Status != PromptStatus.OK) return;
                using (var trans = doc.TransactionManager.StartTransaction())
                {
                    setRs.Value.MassiveSuperExplode(out DBObjectCollection col);
                    foreach (var obj in col) trans.AddNewlyCreatedDBObject(obj as DBObject, true);
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                doc.Editor.WriteMessage($"/n{ex.Message}");
            }
        }
    }
}

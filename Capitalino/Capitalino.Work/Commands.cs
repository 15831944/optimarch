using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Capitalino.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

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
        [CommandMethod("ZAREA", CommandFlags.UsePickSet)]
        public static void ZAREA()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            try
            {
                var complete = false;
                var addCol = new List<double>();
                var subCol = new List<double>();
                var sum = 0d;

                var selOptions = new PromptEntityOptions("\n[增加(a)/减少(s)]");
                selOptions.Keywords.Add("a");
                selOptions.Keywords.Add("s");
                selOptions.Keywords.Default = "a";
                selOptions.AppendKeywordsToMessage = true;

                using (var trans = doc.TransactionManager.StartTransaction())
                {
                    do
                    {
                        var entRs = doc.Editor.GetEntity(selOptions);

                        if (entRs.Status == PromptStatus.Keyword)
                        {
                            switch (entRs.StringResult)
                            {
                                case "a":
                                    break;
                                default:
                                    break;
                            }
                        }


                        switch (key)
                        {
                            default:
                                complete = true;
                                break;
                            case "a":
                                var addRs = doc.Editor.GetSelection(selOptions);

                                if (addRs.Status != PromptStatus.OK) break;
                                var addPls = from id in addRs.Value.GetObjectIds()
                                             let pl = id.GetObject(OpenMode.ForRead) as Polyline
                                             where pl != null && pl.Closed
                                             let layer = pl.LayerId.GetObject(OpenMode.ForRead) as LayerTableRecord
                                             where !layer.IsLocked && !layer.IsFrozen && !layer.IsHidden
                                             select pl.Area;
                                addCol.AddRange(addPls);
                                break;
                            case "s":
                                var subRs = doc.Editor.GetSelection();
                                if (subRs.Status != PromptStatus.OK) break;
                                var subPls = from id in subRs.Value.GetObjectIds()
                                             let pl = id.GetObject(OpenMode.ForRead) as Polyline
                                             where pl != null && pl.Closed
                                             let layer = pl.LayerId.GetObject(OpenMode.ForRead) as LayerTableRecord
                                             where !layer.IsLocked && !layer.IsFrozen && !layer.IsHidden
                                             select pl.Area;
                                subCol.AddRange(subPls); break;
                        }
                    } while (!complete);
                }
                var rs = addCol.Sum() - subCol.Sum();
                rs /= 1000000d;
                Clipboard.SetText($"{Math.Round(rs,2)}");
                doc.Editor.WriteMessage($"\n面积为{Math.Round(rs, 2)}");
            }
            catch (Exception ex)
            {
                doc.Editor.WriteMessage($"/n{ex.Message}");
            }
        }
    }
}

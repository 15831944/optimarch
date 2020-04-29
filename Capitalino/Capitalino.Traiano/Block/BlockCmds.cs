using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Capitalino.Traiano.Block
{
    internal static class BlockCmds
    {
        private static Document doc => Application.DocumentManager.MdiActiveDocument;
        internal static void CountBlockInstance()
        {
            try
            {
                using (var trans = doc.TransactionManager.StartTransaction())
                {
                    var sampleRs = doc.Editor.GetEntity("Pick up a BlockReferencEntity:");
                    if (sampleRs.Status != PromptStatus.OK)
                        return;

                    var sampleId = sampleRs.ObjectId;
                    var sample = trans.GetObject(sampleId, OpenMode.ForRead) as BlockReference;
                    if (sample is null || sample.BlockTableRecord.IsNull)
                    {
                        doc.Editor.WriteMessage("\nNot a valid BlockReference!");
                        return;
                    }

                    var setRs = doc.Editor.GetSelection();
                    if (setRs.Status != PromptStatus.OK) 
                        return;

                    var brs = from id in setRs.Value.GetObjectIds()
                              let br = trans.GetObject(id, OpenMode.ForRead) as BlockReference
                              where br != null && !br.BlockTableRecord.IsNull && br.BlockTableRecord == sample.BlockTableRecord
                              select id;

                    var n = brs.Count();
                    doc.Editor.WriteMessage($"\nThe Count of the identical BlockReference is: {n}");
                    Clipboard.SetText(n.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
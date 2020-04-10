using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace Capitalino.Core
{
    public static class EditorTool
    {
        public static PromptSelectionResult SelectImpliedOrGetSelection(this Editor ed, string message)
        {
            var selectionRs = ed.SelectImplied();

            if (selectionRs.Status == PromptStatus.Error)
            {
                ed.WriteMessage($"\n{message}");
                selectionRs = ed.GetSelection();
                if (selectionRs.Status == PromptStatus.OK)
                {
                    ed.SetImpliedSelection(selectionRs.Value.GetObjectIds());
                }
            }
            return selectionRs;
        }
        public static PromptSelectionResult SelectImpliedOrGetSelection(this Editor ed) => ed.SelectImpliedOrGetSelection("Select Objects:");

        public static Database Database(this SelectionSet set) => set[0].ObjectId.Database;

    }
}


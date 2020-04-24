using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Capitalino.Core.Publication;
using Capitalino.Core.Coordinates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Autodesk.AutoCAD.Windows;
using System.Windows;

namespace Capitalino.Traiano
{
    internal static class PublicationCmds
    {
		private static Document doc = Application.DocumentManager.MdiActiveDocument;
        internal static void PrintSingleFrame()
        {
			try
			{
				var entOptions = new PromptEntityOptions("\nSelect a Frame: [A0(0)/A1(1)/A2(2)/A3(3)/A4(4)]");
				for (int i = 0; i < 5; i++) entOptions.Keywords.Add($"{i}");

				entOptions.Keywords.Default = "3";
				var paper = "A3";

				PromptEntityResult entRs;
				do
				{
					entRs = doc.Editor.GetEntity(entOptions);
					if (entRs.Status == PromptStatus.Keyword)
					{
						paper = $"A{entRs.StringResult}";
						entOptions.Keywords.Default = entRs.StringResult;
					}

				} while (entRs is null || entRs.Status != PromptStatus.OK);



				if (!CptPaperInfo.TryParseToPaperInfo(paper, out CptPaperInfo info))
					return;

				using (var trans = doc.TransactionManager.StartTransaction())
				{
					var frame = trans.GetObject(entRs.ObjectId, OpenMode.ForRead) as Entity;

					if (!frame.Bounds.HasValue)
						return;

					var layout = trans.GetObject(
						LayoutManager.Current.GetLayoutId(LayoutManager.Current.CurrentLayout),
						OpenMode.ForRead) as Layout;

					var engine = new CptPlotEngine(layout, CptPlotConfigParams.PlotConfigName);

					var style = engine.PlotStyleSheetNameList
						.FirstOrDefault(x => x.ToUpper().Contains(info.BasicName));
					if (style is null)
						style = "monochrome.ctb";

					var canon = engine.CanonicalMediaNameList
						.FirstOrDefault(x => x.Contains(CptPlotConfigParams.GetCanonicalMediaNamePattern(info)));


					var plotOptions = new CptPlotOptions
					{
						PlotWindowArea = ((Extents3d)frame.Bounds).ReduceDimensions(),
						CanonicalMediaName = canon,
						CurrentStyleSheet = style,
						PlotRotation = PlotRotation.Degrees000,
						ScaleToFit = true
					};

					var dialog = new SaveFileDialog(
						"保存PDF",
						"plot.pdf", 
						"pdf", 
						"Capitalino",
						SaveFileDialog.SaveFileDialogFlags.DefaultIsFolder);
					dialog.ShowModal();
					var path = dialog.Filename;

					engine.PlotToPDF(plotOptions, path, "Printing...");

				}

			}
			catch (Exception ex)
			{

				MessageBox.Show(ex.Message);
			}
        }
    }
}

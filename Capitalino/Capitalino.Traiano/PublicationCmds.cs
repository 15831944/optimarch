using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using Capitalino.Core.Coordinates;
using Capitalino.Core.Publication;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Capitalino.Traiano
{
	internal static class PublicationCmds
    {
		private static Document doc => Application.DocumentManager.MdiActiveDocument;
        internal static void PrintSingle()
        {
			try
			{
				var pm = new PrintSingleParameters();

				var entOptions = new PromptEntityOptions("\nSet Parameters:");
				entOptions.Keywords.Add("P", "P", "Paper(P)");
				entOptions.Keywords.Add("S", "S", "Scale(S)");
				entOptions.Keywords.Add("D", "D", "Direction(D)");

				getentity:
				var entRs = doc.Editor.GetEntity(entOptions);
				switch (entRs.Status)
				{
					case PromptStatus.Keyword:
						switch (entRs.StringResult)
						{
							case "P":
								var paperOptions = new PromptKeywordOptions("Select Paper:");
								for (int i = 0; i < 5; i++)
									paperOptions.Keywords.Add($"{i}", $"{i}", $"A{i}({i})");

								paperOptions.Keywords.Default = pm.Paper.Substring(1,1);
								var paperRs = doc.Editor.GetKeywords(paperOptions);
								if (paperRs.Status != PromptStatus.OK) return;
								pm.Paper = $"A{paperRs.StringResult}";
								break;
							case "S":
								var fitOptions = new PromptKeywordOptions("Use ScaleToFit?");
								fitOptions.Keywords.Add("Y", "Y", "Yes(Y)");
								fitOptions.Keywords.Add("N", "N", "No(N)");
								fitOptions.Keywords.Default = pm.ScaleToFit ? "Y" : "N";

								var fitRs = doc.Editor.GetKeywords(fitOptions);
								if (fitRs.Status != PromptStatus.OK) return;
								switch (fitRs.StringResult)
								{
									default:
									case "Y":
										pm.ScaleToFit = true;
										break;
									case "N":
										var scaleOptions = new PromptStringOptions($"Input Scale 1 : {pm.CustomScale}?");
										var scaleRs = doc.Editor.GetString(scaleOptions);
										if (double.TryParse(scaleRs.StringResult, out double sc))
										{
											pm.ScaleToFit = false;
											pm.CustomScale = sc;
										}
										break;
								}
								break;
							case "D":
								var dirOptions = new PromptKeywordOptions("Choose Direction:");
								dirOptions.Keywords.Add("H", "H", "Horizontal(H)");
								dirOptions.Keywords.Add("V", "V", "Vertical(V)");
								dirOptions.Keywords.Add("A", "A", "Auto(A)");

								switch (pm.Direction)
								{
									case CptPrintDirection.Horizontal:
										dirOptions.Keywords.Default = "H";
										break;
									case CptPrintDirection.Vertical:
										dirOptions.Keywords.Default = "V";
										break;
									case CptPrintDirection.Auto:
										dirOptions.Keywords.Default = "A";
										break;
									default:
										return;
								}

								var dirRs = doc.Editor.GetKeywords(dirOptions);
								if (dirRs.Status != PromptStatus.OK) return;

								switch (dirRs.StringResult)
								{
									default:
									case "A":
										pm.Direction = CptPrintDirection.Auto;
										break;
									case "H":
										pm.Direction = CptPrintDirection.Horizontal;
										break;
									case "V":
										pm.Direction = CptPrintDirection.Vertical;
										break;
								}
								break;
							default:
								break;
						}
						goto getentity;
					case PromptStatus.OK:
						break;
					default:
						return;
				}


				if (!CptPaperInfo.TryParseToPaperInfo(pm.Paper, out CptPaperInfo info))
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

					var area = ((Extents3d)frame.Bounds).ReduceDimensions();


					var plotOptions = new CptPlotOptions
					{
						PlotWindowArea = area,
						CanonicalMediaName = canon,
						CurrentStyleSheet = style,
					};

					switch (pm.Direction)
					{
						case CptPrintDirection.Vertical:
							plotOptions.PlotRotation = PlotRotation.Degrees090;
							break;
						case CptPrintDirection.Horizontal:
							plotOptions.PlotRotation = PlotRotation.Degrees000;
							break;
						case CptPrintDirection.Auto:
							var h = Math.Abs(area.MaxPoint.Y - area.MinPoint.Y);
							var w = Math.Abs(area.MaxPoint.X - area.MinPoint.X);
							plotOptions.PlotRotation = h > w ? PlotRotation.Degrees090 : PlotRotation.Degrees000;
							break;
						default:
							return;
					}

					if (pm.ScaleToFit)
					{
						plotOptions.ScaleToFit = true;
					}
					else
					{
						plotOptions.ScaleToFit = false;
						plotOptions.CustomPrintScale = new CustomScale(1, pm.CustomScale);
					}


					var dialog = new SaveFileDialog(
						"保存PDF",
						"plot.pdf", 
						"pdf", 
						"Capitalino",
						SaveFileDialog.SaveFileDialogFlags.DefaultIsFolder);
					if (!(bool)dialog.ShowModal()) return;
					var path = dialog.Filename;

					engine.PlotToPDF(plotOptions, path, "Printing...");

				}

			}
			catch (Exception ex)
			{

				MessageBox.Show(ex.Message);
			}
        }

		internal static void PrintMulti()
		{
			try
			{
				var pm = new PrintSingleParameters();
				PromptSelectionResult setRs;
				Menu:
				var menuOptions = new PromptKeywordOptions("\nSet Parameters:");
				menuOptions.Keywords.Add("P", "P", "Paper(P)");
				menuOptions.Keywords.Add("S", "S", "Scale(S)");
				menuOptions.Keywords.Add("D", "D", "Direction(D)");
				menuOptions.Keywords.Add("A", "A", "AddEntities(A)");

				var menuRs = doc.Editor.GetKeywords(menuOptions);
				if (menuRs.Status != PromptStatus.OK) return;

				switch (menuRs.StringResult)
				{
					case "P":
						var paperOptions = new PromptKeywordOptions("Select Paper:");
						for (int i = 0; i < 5; i++)
							paperOptions.Keywords.Add($"{i}", $"{i}", $"A{i}({i})");

						paperOptions.Keywords.Default = pm.Paper.Substring(1, 1);
						var paperRs = doc.Editor.GetKeywords(paperOptions);
						if (paperRs.Status != PromptStatus.OK) return;
						pm.Paper = $"A{paperRs.StringResult}";
						goto Menu;
					case "S":
						var fitOptions = new PromptKeywordOptions("Use ScaleToFit?");
						fitOptions.Keywords.Add("Y", "Y", "Yes(Y)");
						fitOptions.Keywords.Add("N", "N", "No(N)");
						fitOptions.Keywords.Default = pm.ScaleToFit ? "Y" : "N";

						var fitRs = doc.Editor.GetKeywords(fitOptions);
						if (fitRs.Status != PromptStatus.OK) return;
						switch (fitRs.StringResult)
						{
							default:
							case "Y":
								pm.ScaleToFit = true;
								break;
							case "N":
								var scaleOptions = new PromptStringOptions($"Input Scale 1 : {pm.CustomScale}?");
								var scaleRs = doc.Editor.GetString(scaleOptions);
								if (double.TryParse(scaleRs.StringResult, out double sc))
								{
									pm.ScaleToFit = false;
									pm.CustomScale = sc;
								}
								break;
						}
						goto Menu;
					case "D":
						var dirOptions = new PromptKeywordOptions("Choose Direction:");
						dirOptions.Keywords.Add("H", "H", "Horizontal(H)");
						dirOptions.Keywords.Add("V", "V", "Vertical(V)");
						dirOptions.Keywords.Add("A", "A", "Auto(A)");

						switch (pm.Direction)
						{
							case CptPrintDirection.Horizontal:
								dirOptions.Keywords.Default = "H";
								break;
							case CptPrintDirection.Vertical:
								dirOptions.Keywords.Default = "V";
								break;
							case CptPrintDirection.Auto:
								dirOptions.Keywords.Default = "A";
								break;
							default:
								return;
						}

						var dirRs = doc.Editor.GetKeywords(dirOptions);
						if (dirRs.Status != PromptStatus.OK) return;

						switch (dirRs.StringResult)
						{
							default:
							case "A":
								pm.Direction = CptPrintDirection.Auto;
								break;
							case "H":
								pm.Direction = CptPrintDirection.Horizontal;
								break;
							case "V":
								pm.Direction = CptPrintDirection.Vertical;
								break;
						}
						goto Menu;
					case "A":
						setRs = doc.Editor.GetSelection();
						if (setRs.Status != PromptStatus.OK) return;
						break;
					default:
						return;
				}
				if (!CptPaperInfo.TryParseToPaperInfo(pm.Paper, out CptPaperInfo info))
					return;

				using (var trans = doc.TransactionManager.StartTransaction())
				{
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

					var ents = from id in setRs.Value.GetObjectIds()
							   let ent = trans.GetObject(id, OpenMode.ForRead) as Entity
							   where ent != null && ent.Bounds.HasValue
							   select ent;

					var plotOptions = new CptPlotOptions
					{
						CanonicalMediaName = canon,
						CurrentStyleSheet = style,
					};

					if (pm.ScaleToFit)
					{
						plotOptions.ScaleToFit = true;
					}
					else
					{
						plotOptions.ScaleToFit = false;
						plotOptions.CustomPrintScale = new CustomScale(1, pm.CustomScale);
					}

					var dialog = new SaveFileDialog(
						"保存PDF",
						"plot.pdf", 
						"pdf", 
						"Capitalino",
						SaveFileDialog.SaveFileDialogFlags.DefaultIsFolder);
					if (!(bool)dialog.ShowModal()) return;
					var path = dialog.Filename;

					var n = ents.Count();
					for (int i = 0; i< n; i++)
					{
						var frame = ents.ElementAt(i);
						var area = ((Extents3d)frame.Bounds).ReduceDimensions();

						plotOptions.PlotWindowArea = area;

						switch (pm.Direction)
						{
							case CptPrintDirection.Vertical:
								plotOptions.PlotRotation = PlotRotation.Degrees090;
								break;
							case CptPrintDirection.Horizontal:
								plotOptions.PlotRotation = PlotRotation.Degrees000;
								break;
							case CptPrintDirection.Auto:
								var h = Math.Abs(area.MaxPoint.Y - area.MinPoint.Y);
								var w = Math.Abs(area.MaxPoint.X - area.MinPoint.X);
								plotOptions.PlotRotation = h > w ? PlotRotation.Degrees090 : PlotRotation.Degrees000;
								break;
							default:
								return;
						}
						var dir = Path.GetDirectoryName(path);
						var preffix = Path.GetFileNameWithoutExtension(path);
						var suffix = Path.GetExtension(path);
						engine.PlotToPDF(
							plotOptions,
							$"{dir}\\{preffix}_{(i+1):000}{suffix}",
							$"{i + 1}/{n} Printing...");
					}
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
	}
}

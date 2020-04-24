using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;
using Capitalino.Core.Coordinates;
using System;
using PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType;

namespace Capitalino.Core.Publication
{
    public class CptPlotEngine
    {
        private readonly Layout layout;
        private readonly short defaultbkpltVar;
        private const string plotconfig = "AutoCad PDF (High Quality Print).pc3";
        public string[] CanonicalMediaNameList { get; }
        public string[] PlotStyleSheetNameList { get; }

        public CptPlotEngine(Layout layout)
        {
            this.layout = layout;
            defaultbkpltVar = (short)Application.GetSystemVariable("BACKGROUNDPLOT");
            using (var ps = new PlotSettings(layout.ModelType))
            {
                ps.CopyFrom(layout);
                var psv = PlotSettingsValidator.Current;
                psv.SetPlotConfigurationName(ps, plotconfig, null);
                psv.RefreshLists(ps);
                var mcol = psv.GetCanonicalMediaNameList(ps);
                CanonicalMediaNameList = new string[mcol.Count];
                mcol.CopyTo(CanonicalMediaNameList, 0);
                var scol = psv.GetPlotStyleSheetList();
                PlotStyleSheetNameList = new string[scol.Count];
                scol.CopyTo(PlotStyleSheetNameList, 0);
            }
        }

        public void PlotToPDF(CptPlotOptions options, string path, string title)
        {
            try
            {
                Application.SetSystemVariable("BACKGROUNDPLOT", 0);
                using (var pi = new PlotInfo())
                {
                    pi.Layout = layout.ObjectId;
                    using (var ps = new PlotSettings(layout.ModelType))
                    {
                        ps.CopyFrom(layout);
                        var psv = PlotSettingsValidator.Current;

                        var minpt = options.PlotWindowArea.MinPoint.TransformByTarget();
                        var maxpt = options.PlotWindowArea.MaxPoint.TransformByTarget();

                        var w = maxpt.X - minpt.X;
                        var h = maxpt.Y - minpt.Y;
                        var rotation = w > h ? PlotRotation.Degrees000 : PlotRotation.Degrees090;

                        psv.SetPlotConfigurationName(ps, plotconfig, options.CanonicalMediaName);
                        psv.SetCurrentStyleSheet(ps, options.CurrentStyleSheet);
                        psv.SetPlotWindowArea(ps, new Extents2d(minpt, maxpt));
                        psv.SetPlotType(ps, PlotType.Window);
                        psv.SetPlotCentered(ps, true);
                        psv.SetPlotPaperUnits(ps, PlotPaperUnit.Millimeters);
                        psv.SetPlotRotation(ps, rotation);
                        if (options.ScaleToFit)
                        {
                            psv.SetUseStandardScale(ps, true);
                            psv.SetStdScaleType(ps, StdScaleType.ScaleToFit);
                        }
                        else
                        {
                            psv.SetUseStandardScale(ps, false);
                            psv.SetCustomPrintScale(ps, options.CustomPrintScale);
                        }
                        pi.OverrideSettings = ps;
                        using (var piv = new PlotInfoValidator())
                        {
                            piv.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;
                            piv.Validate(pi);
                            using (var pe = PlotFactory.CreatePublishEngine())
                            {
                                using (var ppd = new PlotProgressDialog(false, 1, false))
                                {
                                    using (var ppi = new PlotPageInfo())
                                    {
                                        ppd.LowerPlotProgressRange = 0;
                                        ppd.UpperPlotProgressRange = 100;
                                        ppd.PlotProgressPos = 0;
                                        ppd.OnBeginPlot();
                                        ppd.IsVisible = true;

                                        pe.BeginPlot(ppd, null);
                                        pe.BeginDocument(pi, layout.Database.OriginalFileName, null, 1, true, path);

                                        ppd.set_PlotMsgString(PlotMessageIndex.DialogTitle, title);
                                        ppd.OnBeginSheet();
                                        ppd.LowerSheetProgressRange = 0;
                                        ppd.UpperSheetProgressRange = 100;
                                        ppd.SheetProgressPos = 0;

                                        pe.BeginPage(ppi, pi, true, null);
                                        pe.BeginGenerateGraphics(null);
                                        ppd.SheetProgressPos = 99;
                                        pe.EndGenerateGraphics(null);

                                        pe.EndPage(null);
                                        ppd.SheetProgressPos = 100;
                                        ppd.OnEndSheet();

                                        pe.EndDocument(null);

                                        ppd.PlotProgressPos = 100;
                                        ppd.OnEndPlot();
                                        pe.EndPlot(null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Application.SetSystemVariable("BACKGROUNDPLOT", defaultbkpltVar);
            }
        }

    }
}

namespace Capitalino.Core.Publication
{
    public static  class CptPlotConfigParams
    {
        public static string PlotConfigName { get => "AutoCad PDF (High Quality Print).pc3"; }
        public static string GetCanonicalMediaNamePattern(CptPaperInfo info)
        {
            if (info.IsExtensive)
            {
                return $"{info.Width:0.00} x {info.Height:0.00}";
            }
            else
            {
                return $"ISO_full_bleed_{info.BasicName}_({info.Width:0.00}_x_{info.Height:0.00}_MM)";
            }
        }
    }
}

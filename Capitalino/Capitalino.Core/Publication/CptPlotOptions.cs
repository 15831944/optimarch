using Autodesk.AutoCAD.DatabaseServices;

namespace Capitalino.Core.Publication
{
    public class CptPlotOptions
    {
        public string Name { get; set; }
        public string CanonicalMediaName { get; set; }
        public string CurrentStyleSheet { get; set; }
        public CustomScale CustomPrintScale { get; set; }
        public bool ScaleToFit { get; set; }
        public Extents2d PlotWindowArea { get; set; }
    }
}

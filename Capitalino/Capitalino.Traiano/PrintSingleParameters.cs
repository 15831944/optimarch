namespace Capitalino.Traiano
{
    public class PrintSingleParameters
    {
        public static PrintSingleParameters Current { get; set; }
        public PrintSingleParameters()
        {
            Paper = "A3";
            ScaleToFit = true;
            Direction = CptPrintDirection.Auto;
        }
        public string Paper { get; set; }
        public bool ScaleToFit { get; set; }
        public double CustomScale { get; set; }
        public CptPrintDirection Direction { get; set; }
    }
    public enum CptPrintDirection
    {
        Vertical,
        Horizontal,
        Auto
    }
}

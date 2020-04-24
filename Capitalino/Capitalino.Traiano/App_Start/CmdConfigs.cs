using Autodesk.AutoCAD.Runtime;
[assembly: CommandClass(typeof(Capitalino.Traiano.CmdConfigs))]
namespace Capitalino.Traiano
{
    public class CmdConfigs
    {
        [CommandMethod("ZPRINTSINGLE")]
        public static void ZPRINTSINGLE() => PublicationCmds.PrintSingleFrame();
    }
}
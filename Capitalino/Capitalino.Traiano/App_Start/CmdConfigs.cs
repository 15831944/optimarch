using Autodesk.AutoCAD.Runtime;
[assembly: CommandClass(typeof(Capitalino.Traiano.CmdConfigs))]
namespace Capitalino.Traiano
{
    public class CmdConfigs
    {
        [CommandMethod("ZPRINTSINGLE")]
        public static void ZPRINTSINGLE() => PublicationCmds.PrintSingle();
        [CommandMethod("ZPRINTMULTI")]
        public static void ZPRINTMULTI() => PublicationCmds.PrintMulti();
    }
}
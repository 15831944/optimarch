using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;
using System.Linq;
[assembly: ExtensionApplication(typeof(Capitalino.Traiano.AppConfigs))]
namespace Capitalino.Traiano
{
    class AppConfigs : IExtensionApplication
    {
        public void Initialize()
        {
            var ribControl = ComponentManager.Ribbon;
            var tab = ribControl.Tabs.Where(x => x.Id == TraiRibbonTab.Id).FirstOrDefault() as TraiRibbonTab;
            if (tab is null)
            {
                tab = new TraiRibbonTab();
                ribControl.Tabs.Add(tab);
            }
        }

        public void Terminate()
        {

        }
    }
}

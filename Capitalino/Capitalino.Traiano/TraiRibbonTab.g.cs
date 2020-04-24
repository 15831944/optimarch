using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitalino.Traiano
{
    partial class TraiRibbonTab
    {
        internal new static string Id => "CPTTRJ";
        private void InitializeComponents()
        {
            base.Id = Id;
            Title = "OPTIMARCH";
            Panels.Add(new RibbonPanel
            {
                Source = new RibbonPanelSource
                {
                    Title = "发布"
                }
            });

            

            var list = new RibbonMenuButton();
            var a0 = new RibbonMenuItem
            {
                Text = "A0",
                ShowText = true,
            };
            list.Items.Add(a0);
            Panels[0].Source.Items.Add(list);
        }
    }
}

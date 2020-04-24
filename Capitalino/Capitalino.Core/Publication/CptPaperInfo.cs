using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitalino.Core.Publication
{
    public class CptPaperInfo
    {
        private CptPaperInfo()
        {

        }
        public string BasicName { get; set; }
        public string FullName { get; set; }
        public bool IsExtensive { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public static bool TryParseToPaperInfo(string paperName, out CptPaperInfo info)
        {
            info = null;

            if (paperName.Length != 2 && paperName.Length != 7) 
                return false;

            var basic = paperName.Split('+')[0];
            var ex = false;
            int w, h;
            switch (basic)
            {
                case "A0":
                    w = 1189;
                    h = 841;
                    break;
                case "A1":
                    w = 841;
                    h = 594;
                    break;
                case "A2":
                    w = 594;
                    h = 420;
                    break;
                case "A3":
                    w = 420;
                    h = 297;
                    break;
                case "A4":
                    w = 297;
                    h = 210;
                    break;
                default:
                    return false;
            }

            if (paperName.Contains('+'))
            {
                var last = paperName.Split('+')[1];

                if (!new string[] { "0.25", "0.50", "0.75", "1.00" }.Contains(last)) 
                    return false;

                var exScale = double.Parse(last);
                w += (int)(exScale * w);
                ex = true;
            }

            info = new CptPaperInfo
            {
                IsExtensive = ex,
                BasicName = basic,
                FullName = paperName,
                Width = w,
                Height = h
            };

            return true;
        }
    }
}

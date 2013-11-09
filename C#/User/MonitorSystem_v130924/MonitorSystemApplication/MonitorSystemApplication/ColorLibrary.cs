using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MonitorSystemApplication
{
    class ColorLibrary
    {
        public static List<Color> colorList = new List<Color>
        {
            Color.Red,
            Color.Blue,
            Color.Green,
            Color.Gray,
            Color.Pink,
            Color.Purple,
            Color.Yellow,
            Color.AliceBlue,
            Color.Beige,
            Color.Brown,
            Color.DarkRed,
            Color.Gold
        };

        public static int index=0;

        public static Color NextColor()
        {
            if (index >= colorList.Count)
            {
                index = 0;
            }

            return colorList[index++];
        }
    }
}

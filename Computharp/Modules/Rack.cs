using ColorHelper;
using System.Collections.Generic;
using System.Windows.Media;

namespace Computharp.Modules
{
    public static class Rack
    {
        private static ComputharpDmiBox dmibox;
        public static ComputharpDmiBox DMIBox { get => dmibox; set => dmibox = value; }

        public static SolidColorBrush BrushNeutral = new SolidColorBrush(Color.FromArgb(KEYBASEALPHA, 0, 0, 0));
        public static SolidColorBrush BrushActivePrimary = new SolidColorBrush(Color.FromArgb(KEYACTIVEALPHA, 0xFF, 0xFF, 0xFF));
        public static SolidColorBrush BrushActiveSecondary = new SolidColorBrush(Color.FromArgb(KEYACTIVEALPHA, 0xBF, 0xBF, 0xBF));
        public static SolidColorBrush BrushScale = new SolidColorBrush(Colors.DarkRed);

        public const byte KEYBASEALPHA = 220;
        public const byte KEYACTIVEALPHA = 190;
        public const byte KEYSAT = 100;
        public const byte KEYLUM = 10;

        public readonly static List<HSL> ScaleColors = new List<HSL>()
        {
            new HSL(0, KEYSAT, KEYLUM),
            new HSL(30, KEYSAT, KEYLUM),
            new HSL(60, KEYSAT, KEYLUM),
            new HSL(105, KEYSAT, KEYLUM),
            new HSL(180, KEYSAT, KEYLUM),
            new HSL(230, KEYSAT, KEYLUM),
            new HSL(290, KEYSAT, KEYLUM),
        };

        //public readonly static List<Color> ScaleColors = new List<Color>()
        //{
            //Color.FromRgb(0x7F, 0x00, 0x00),
            //Color.FromRgb(0x7F, 0x41, 0x00),
            //Color.FromRgb(0x7F, 0x79, 0x00),
            //Color.FromRgb(0x1F, 0x7F, 0x00),
            //Color.FromRgb(0x00, 0x7F, 0x7F),
            //Color.FromRgb(0x04, 0x00, 0x7F),
            //Color.FromRgb(0x74, 0x00, 0x7F),

            // Plain newpalette colors with Alpha

            //Color.FromArgb(keyAlpha, 0x7F, 0x00, 0x00),
            //Color.FromArgb(keyAlpha, 0x7F, 0x41, 0x00),
            //Color.FromArgb(keyAlpha, 0x7F, 0x79, 0x00),
            //Color.FromArgb(keyAlpha, 0x1F, 0x7F, 0x00),
            //Color.FromArgb(keyAlpha, 0x00, 0x7F, 0x7F),
            //Color.FromArgb(keyAlpha, 0x04, 0x00, 0x7F),
            //Color.FromArgb(keyAlpha, 0x74, 0x00, 0x7F),

            // Other crap which doesn't work

            //ColorSpaces.FromAHSV(keyAlpha, 0, keySat, keyVal),
            //ColorSpaces.FromAHSV(keyAlpha, 30, keySat, keyVal),
            //ColorSpaces.FromAHSV(keyAlpha, 60, keySat, keyVal),
            //ColorSpaces.FromAHSV(keyAlpha, 105, keySat, keyVal),
            //ColorSpaces.FromAHSV(keyAlpha, 180, keySat, keyVal),
            //ColorSpaces.FromAHSV(keyAlpha, 240, keySat, keyVal),
            //ColorSpaces.FromAHSV(keyAlpha, 290, keySat, keyVal),
        //};
    }
}

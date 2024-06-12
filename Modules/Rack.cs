using ColorHelper;
using System.Windows.Media;

namespace Computharp.Modules
{
    public static class Rack
    {
        private static ComputharpDmiBox dmibox;
        

        public static ComputharpDmiBox DMIBox { get => dmibox; set => dmibox = value; }

        // ============================================================================

        public static readonly SolidColorBrush BrushNeutral = new(Color.FromArgb(KEYBASEALPHA, 0, 0, 0));
        public static readonly SolidColorBrush BrushActivePrimary = new(Color.FromArgb(KEYACTIVEALPHA, 0xFF, 0xFF, 0xFF));
        public static readonly SolidColorBrush BrushActiveSecondary = new(Color.FromArgb(KEYACTIVEALPHA, 0x9F, 0x9F, 0x9F));
        public static readonly SolidColorBrush BrushScale = new(Colors.DarkRed);
        public static readonly SolidColorBrush BrushDrone = new(Color.FromArgb(KEYACTIVEALPHA, 0x5F, 0x5F, 0x5F));

        public static readonly SolidColorBrush BrushMajor = new(Color.FromRgb(0x33, 0x00, 0x00));
        public static readonly SolidColorBrush BrushMinor = new(Color.FromRgb(0x00, 0x08, 0x33));

        public static readonly SolidColorBrush BrushMiddle = new(Color.FromRgb(0x33, 0x33, 0x33));

        public static readonly SolidColorBrush BrushDefaultString = new(Color.FromRgb(0xEE, 0xEE, 0xEE));
        
        public const byte KEYBASEALPHA = 255;
        public const byte KEYACTIVEALPHA = 255;
        public const byte KEYSAT = 100;
        public const byte KEYLUM = 10;

        public const string DEFAULTVOIDSTRING = "-";

        public readonly static List<HSL> ScaleColors = new()
        {
            new HSL(0, KEYSAT, KEYLUM),
            new HSL(30, KEYSAT, KEYLUM),
            new HSL(60, KEYSAT, KEYLUM),
            new HSL(105, KEYSAT, KEYLUM),
            new HSL(180, KEYSAT, KEYLUM),
            new HSL(230, KEYSAT, KEYLUM),
            new HSL(290, KEYSAT, KEYLUM),
        };
    }
}

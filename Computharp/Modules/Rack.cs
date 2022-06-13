using System.Windows.Media;

namespace Computharp.Modules
{
    public static class Rack
    {
        private static ComputharpDmiBox dmibox;
        public static ComputharpDmiBox DMIBox { get => dmibox; set => dmibox = value; }

        public static SolidColorBrush BrushNeutral = new SolidColorBrush(Colors.Black);
        public static SolidColorBrush BrushActivePrimary = new SolidColorBrush(Colors.GreenYellow);
        public static SolidColorBrush BrushActiveSecondary = new SolidColorBrush(Colors.Green);
        public static SolidColorBrush BrushScale = new SolidColorBrush(Colors.DarkRed);
    }
}

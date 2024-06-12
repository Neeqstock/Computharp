using NITHdmis.Music;
using RawInputProcessor;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Computharp.Modules
{
    public class KKey
    {
        public string Text { get; set; }
        public Key Key { get; set; }
        public KeyPressState KeyPressState { get; set; }
        public MidiNotes MidiNote { get; set; }
        public KKeyTypes KKeyType { get; set; }
        public Button Button { get; set; }
        public SolidColorBrush BaseBackground { get; set; } = Rack.BrushNeutral;
    }
}

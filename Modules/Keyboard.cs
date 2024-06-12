using RawInputProcessor;

namespace Computharp.Modules
{
    public class Keyboard
    {
        public int Octave { get; set; } = 0;
        public int Transp { get; set; } = 0;
        public RawKeyboardDevice RawDevice { get; set; }
        public List<List<KKey>> NoteKeys { get; set; } = new();
        public List<KKey> FunKeys { get; set; } = new();

    }
}

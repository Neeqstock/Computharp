using Computharp.Modules;
using NeeqDMIs.Keyboard;
using RawInputProcessor;

namespace Computharp.Behaviors
{
    internal class KBlistenKeystrokes : IKeyboardBehavior
    {
        public int ReceiveEvent(RawInputEventArgs e)
        {
            Rack.DMIBox.ReceiveKeyStroke(e);
            return 0;
        }
    }
}

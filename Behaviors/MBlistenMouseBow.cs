using Computharp.Modules;
using NITHdmis.Modules.Mouse;

namespace Computharp.Behaviors
{
    internal class MBlistenMouseBow : IMouseBehavior
    {
        private int LastDirectionY = 1;
        private bool directionChange = false;
        public int ReceiveSample(MouseModuleSample sample)
        {
            if(sample.DirectionY != LastDirectionY)
            {
                directionChange = true;
            }
            else
            {
                directionChange = false;
            }

            LastDirectionY = sample.DirectionY;

            // todo: Using only Y direction but sqrt of both for speed...
            Rack.DMIBox.ReceiveBowGesture(Math.Sqrt((sample.VelocityY * sample.VelocityY + sample.VelocityX * sample.VelocityX)), directionChange);
            return 0;
        }
    }
}

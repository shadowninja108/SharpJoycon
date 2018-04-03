using System;
using static SharpJoycon.Interfaces.ConfigurationInterface;

namespace SharpJoycon.Interfaces.Joystick.Controllers
{
    public abstract class Joycon : Controller
    {
        protected Buttons buttons;
        protected Position pos;
        protected AnalogConfiguration analogConfirguration;
        protected StickParameters stickParameters;

        public Joycon(NintendoController controller) : base(controller)
        {

        }

        public override void Poll(HIDInterface.PacketData data)
        {
            buttons = new Buttons();

            byte[] bytes = data.rawData;
            int offset = GetStickDataOffset();

            int posX = bytes[offset] | ((bytes[offset + 1] & 0xF) << 8);
            int posY = (bytes[offset + 1] >> 4) | (bytes[offset + 2] << 4);

            AnalogConfiguration config = GetAnalogConfiguration();
            StickParameters stickParameters = GetStickParameters();

            float xDiff = posX - config.xCenter;
            float yDiff = posY - config.yCenter;

            float posXf = xDiff / ((xDiff > 0) ? (config.xMax - config.xCenter) : (config.xCenter - config.xMin));
            float posYf = yDiff / ((yDiff > 0) ? (config.yMax - config.yCenter) : (config.yCenter - config.yMin));
            
            // distance from origin
            if (Math.Sqrt(Math.Pow(xDiff, 2) + Math.Pow(yDiff, 2)) < stickParameters.deadzone) {
                posXf = 0;
                posYf = 0;
            }

            posXf++;
            posXf /= 2;
            posXf = Math.Abs(posXf); // make sure it never goes negative because it will wrap around and glitch
            posYf++;
            posYf /= 2;
            posYf = Math.Abs(posYf);

            posYf = 1 - posYf; // easy way of inverting axis

            posX = (int)(posXf * 35900f);
            posY = (int)(posYf * 35900f);


            pos = new Position(posX, posY);
        }

        public abstract int GetStickDataOffset();

        public override Buttons GetButtons()
        {
            return buttons;
        }

        public override Position GetStick(int id)
        {
            if (id == 0)
            {
                return pos;
            }
            else
            {
                return new Position();
            }
        }

        private AnalogConfiguration GetAnalogConfiguration()
        {
            if (analogConfirguration.Equals(default(AnalogConfiguration)))
            {
                // will eventually add detection for User generated config
                analogConfirguration = controller.GetConfig().GetAnalogConfiguration(ConfigurationType.Factory);
            }
            return analogConfirguration;
        }

        private StickParameters GetStickParameters()
        {
            if (stickParameters.Equals(default(StickParameters)))
            {
                // will eventually add detection for User generated config
                stickParameters = controller.GetConfig().GetStickParameters();
            }
            return stickParameters;
        }

    }
}

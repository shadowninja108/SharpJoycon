using System;
using static SharpJoycon.Interfaces.ConfigurationInterface;
using static SharpJoycon.Interfaces.HIDInterface;

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

        public override void Poll(PacketData data)
        {
            buttons = new Buttons();
            pos = ParseStickPosition(data, 0);
        }

        public Position ParseStickPosition(PacketData data, int id)
        {
            AnalogConfiguration config = GetAnalogConfiguration(id);
            byte[] bytes = data.rawData;
            int offset = GetStickDataOffset();

            int posX = bytes[offset] | ((bytes[offset + 1] & 0xF) << 8);
            int posY = (bytes[offset + 1] >> 4) | (bytes[offset + 2] << 4);

            StickParameters stickParameters = GetStickParameters();

            float xDiff = posX - config.xCenter;
            float yDiff = posY - config.yCenter;

            float posXf = xDiff / ((xDiff > 0) ? (config.xMax - config.xCenter) : (config.xCenter - config.xMin));
            float posYf = yDiff / ((yDiff > 0) ? (config.yMax - config.yCenter) : (config.yCenter - config.yMin));

            // distance from origin
            if (Math.Sqrt(Math.Pow(xDiff, 2) + Math.Pow(yDiff, 2)) < stickParameters.deadzone)
            {
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


            return new Position(posX, posY);
        }

        public abstract int GetStickDataOffset();

        public override Position GetStick(int id)
        {
            if (id == 0)
                return pos;
            return default(Position);
        }

        public override Buttons GetButtons()
        {
            return buttons;
        }

        private AnalogConfiguration GetAnalogConfiguration(int id)
        {
            if (analogConfirguration.Equals(default(AnalogConfiguration)))
            {
                // will eventually add detection for User generated config
                analogConfirguration = controller.GetConfig().GetAnalogConfiguration(id, ConfigurationType.Factory);
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

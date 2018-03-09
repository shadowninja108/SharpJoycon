using System;
using System.Collections;
using System.Linq;
using static SharpJoycon.Interfaces.ConfigurationInterface;
using static SharpJoycon.Interfaces.HIDInterface;

namespace SharpJoycon.Interfaces.Joystick.Controllers
{
    public class LeftJoycon : Controller
    {

        public LeftJoycon(NintendoController controller) : base(controller)
        {
        }

        private Buttons buttons;
        private StickPos pos;
        private AnalogConfiguration analogConfirguration;

        public override void Poll(PacketData data)
        {
            byte[] bytes = data.header.Concat(data.data).ToArray();

            //reset to default values
            buttons = new Buttons();
            if (bytes.Length > 0)
            {
                BitArray leftData = new BitArray(new byte[] { bytes[5] });
                if (leftData.Length >= 7)
                {
                    buttons.povDown = leftData[0];
                    buttons.povUp = leftData[1];
                    buttons.povRight = leftData[2];
                    buttons.povLeft = leftData[3];
                    buttons.SR = leftData[4];
                    buttons.SL = leftData[5];
                    buttons.L = leftData[6];
                    buttons.ZL = leftData[7];
                }
                BitArray sharedData = new BitArray(new byte[] { bytes[4] });
                if (sharedData.Length >= 7)
                {
                    buttons.minus = sharedData[0];
                    buttons.stickL = sharedData[3];
                    buttons.capture = sharedData[5];
                }
                int offset = 6;
                int posX = bytes[offset] | ((bytes[offset + 1] & 0xF) << 8);
                int posY = (bytes[offset] >> 4) | (bytes[offset + 2] << 4);

                AnalogConfiguration config = GetAnalogConfiguration();

                float posXf = (posX - config.xMin) / (float) config.xMax;
                float posYf = (posY - config.yMin) / (float) config.yMax;
                posYf = 1 - posYf; // invert axis
                posX = (int) (posXf * 32767f);
                posY = (int) (posYf * 32767f);

                // testing reads
               // Console.WriteLine($"xMin: {config.xMin} | xCenter: {config.xCenter} | xMax: {config.xMax}");
               // Console.WriteLine($"yMin: {config.yMin} | yCenter: {config.yCenter} | yMax: {config.yMax}");
              //  Console.WriteLine($"posX: {posX} | posY: {posY}");

                pos = new StickPos(posX, posY);
            }
        }

        public override Buttons GetButtons()
        {
            return buttons;
        }

        public override StickPos GetStick(int id)
        {
            if(id == 0)
            {
                return pos;
            } else
            {
                return new StickPos();
            }
        }

        public override POVDirection GetPov(int id)
        {
            if (id == 0)
            {
                bool up = GetButtons().povUp;
                bool down = GetButtons().povDown;
                bool left = GetButtons().povLeft;
                bool right = GetButtons().povRight;

                if (up) return POVDirection.Up;
                if (down) return POVDirection.Down;
                if (left) return POVDirection.Left;
                if (right) return POVDirection.Right;
                return POVDirection.None;
            }
            return POVDirection.None;
        }

        private AnalogConfiguration GetAnalogConfiguration()
        {
            if (analogConfirguration.Equals(default(AnalogConfiguration)))
            {
                // will eventually add detection for User generated config
                analogConfirguration = controller.GetConfig().GetAnalogConfiguration(ConfigurationType.User);
            }
            return analogConfirguration;
        }

        public override int GetAnalogConfigOffset(ConfigurationType type)
        {
            switch (type)
            {
                case ConfigurationType.Factory:
                    return 0x603D;
                case ConfigurationType.User:
                    return 0x8012;
                default:
                    goto case ConfigurationType.Factory;
            }
        }

        public override AnalogConfiguration ParseAnalogConfiguration(int[] data)
        {
            AnalogConfiguration config = new AnalogConfiguration();

            config.xCenter = data[2];
            config.yCenter = data[3];
            config.xMax = data[0] + config.xCenter;
            config.yMax = data[1] + config.yCenter;
            config.xMin = config.xCenter - data[4];
            config.yMin = config.yCenter - data[5];

            return config;
        }
    }
}

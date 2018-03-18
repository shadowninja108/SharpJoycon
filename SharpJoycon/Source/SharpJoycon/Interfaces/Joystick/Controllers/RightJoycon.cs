using System.Collections;
using System.Collections.Generic;
using System.Linq;

using static SharpJoycon.Interfaces.ConfigurationInterface;
using static SharpJoycon.Interfaces.HIDInterface;

namespace SharpJoycon.Interfaces.Joystick.Controllers
{
    class RightJoycon : Controller 
    {
        private Buttons buttons;
        private StickPos pos;
        private AnalogConfiguration analogConfirguration;

        public RightJoycon(NintendoController controller) : base(controller)
        {
        }

        public override Buttons GetButtons()
        {
            return buttons;
        }

        public override List<POVDirection> GetPov(int id)
        {
            return new List<POVDirection>();
        }

        public override StickPos GetStick(int id)
        {
            if (id == 0)
            {
                return pos;
            }
            else
            {
                return new StickPos();
            }
        }

        public override void Poll(PacketData data) 
        {
            //reset to default values
            buttons = new Buttons();
            byte[] bytes = data.header.Concat(data.data).ToArray();
            if (bytes.Length > 0)
            {
                BitArray rightData = new BitArray(new byte[] { bytes[3] });
                if (rightData.Length >= 7)
                {
                    buttons.Y = rightData[0];
                    buttons.X = rightData[1];
                    buttons.B = rightData[2];
                    buttons.A = rightData[3];
                    buttons.SR = rightData[4];
                    buttons.SL = rightData[5];
                    buttons.R = rightData[6];
                    buttons.ZR = rightData[7];

                }
                BitArray sharedData = new BitArray(new byte[] { bytes[4] });
                if (sharedData.Length >= 7)
                {
                    buttons.plus = sharedData[1];
                    buttons.stickR = sharedData[2];
                    buttons.home = sharedData[4];

                }
                int offset = 9;
                int posX = bytes[offset] | ((bytes[offset + 1] & 0xF) << 8);
                int posY = (bytes[offset] >> 4) | (bytes[offset + 2] << 4);

                AnalogConfiguration config = GetAnalogConfiguration();

                float posXf = (posX - config.xMin) / (float)config.xMax;
                float posYf = (posY - config.yMin) / (float)config.yMax;
                posYf = 1 - posYf;
                posX = (int)(posXf * 35900f);
                posY = (int)(posYf * 35900f);

                pos = new StickPos(posX, posY);
            }
        }

        private AnalogConfiguration GetAnalogConfiguration()
        {
            if(analogConfirguration.Equals(default(AnalogConfiguration)))
            {
                // will eventually add detection for User generated config
                analogConfirguration =  controller.GetConfig().GetAnalogConfiguration(ConfigurationType.Factory);
            }
            return analogConfirguration;
        }

        public override AnalogConfiguration ParseAnalogConfiguration(int[] data)
        {
            AnalogConfiguration config = new AnalogConfiguration();

            config.xCenter = data[0];
            config.yCenter = data[1];
            config.xMax = data[4] + config.xCenter;
            config.yMax = data[5] + config.yCenter;
            config.xMin = config.xCenter - data[2];
            config.yMin = config.yCenter - data[3];

            return config;
        }

        public override int GetAnalogConfigOffset(ConfigurationType type)
        {
            switch (type)
            {
                case ConfigurationType.Factory:
                    return 0x6046;
                case ConfigurationType.User:
                    return 0x801D;
                default:
                    goto case ConfigurationType.Factory;
            }
        }
    }
}

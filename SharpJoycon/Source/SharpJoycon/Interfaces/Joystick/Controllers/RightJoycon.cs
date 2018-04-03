using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using static SharpJoycon.Interfaces.ConfigurationInterface;
using static SharpJoycon.Interfaces.HIDInterface;

namespace SharpJoycon.Interfaces.Joystick.Controllers
{
    class RightJoycon : Joycon 
    {
        public RightJoycon(NintendoController controller) : base(controller)
        {
        }

        public override POVDirection GetPov(int id)
        {
            return POVDirection.None;
        }


        public override void Poll(PacketData data) 
        {
            base.Poll(data);

            byte[] bytes = data.rawData;
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
            }
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

        public override int GetStickDataOffset()
        {
            return 9;
        }

        public override int GetStickConfigOffset(ConfigurationType type)
        {
            return GetRightStickConfigOffset(type);
        }
    }
}

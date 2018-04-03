using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static SharpJoycon.Interfaces.ConfigurationInterface;
using static SharpJoycon.Interfaces.HIDInterface;

namespace SharpJoycon.Interfaces.Joystick.Controllers
{
    public class LeftJoycon : Joycon
    {

        public LeftJoycon(NintendoController controller) : base(controller)
        {
        }

        public override void Poll(PacketData data)
        {
            base.Poll(data);

            byte[] bytes = data.rawData;
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

                // pretty shitty but it works
                if (up)
                {
                    if (right)
                        return POVDirection.UpRight;
                    if (left)
                        return POVDirection.LeftUp;
                    return POVDirection.Up;
                }
                if (right)
                {
                    if (down)
                        return POVDirection.RightDown;
                    return POVDirection.Right;
                }
                if (down)
                {
                    if (left)
                        return POVDirection.DownLeft;
                    return POVDirection.Down;
                }
                if (left)
                    return POVDirection.Left;
            }
            return POVDirection.None;
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

        public override int GetStickDataOffset()
        {
            return 6;
        }

        public override int GetStickConfigOffset(ConfigurationType type)
        {
            return GetLeftStickConfigOffset(type);
        }
    }
}

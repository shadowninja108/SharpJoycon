using HidLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joycon_Glue.Source.Joystick
{
    public class LeftJoycon : NintendoController
    {

        public LeftJoycon(SimplifiedHidDevice hid) : base(hid)
        {
            stickConfig = GetAnalogConfiguration(ConfigurationType.Factory);
        }

        private Buttons buttons;
        private StickPos pos;
        private AnalogConfiguration stickConfig;

        public override void Poll(HidDeviceData data)
        {
            //reset to default values
            buttons = new Buttons();
            byte[] bytes = data.Data;
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

                float posXf = (posX - stickConfig.xMin) / (float) stickConfig.xMax;
                float posYf = (posY - stickConfig.yMin) / (float) stickConfig.yMax;
                posYf = 1 - posYf;
                posX = (int) (posXf * 32767f);
                posY = (int) (posYf * 32767f);

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

        public override void Poll()
        {
            base.Poll();
            Task<HidDeviceData> task = hid.GetHidDevice().ReadAsync();
            task.ContinueWith(_ =>
            {
                Poll(task.Result);
            });
        }

        public override POVDirection GetPov()
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

        public override AnalogConfiguration GetAnalogConfiguration(ConfigurationType type)
        {

            uint offset;

            switch (type)
            {
                case ConfigurationType.Factory:
                    offset = 0x603D;
                    break;
                case ConfigurationType.User:
                    offset = 0x8012;
                    break;
                default:
                    offset = 0x603D;
                    break;
            }

            byte[] data = ReadSPI(offset, 0x12);

            Console.WriteLine($"Raw analog config: {BitConverter.ToString(data)}");

            int[] parsed = ParseAnalogConfiguration(data);

            AnalogConfiguration config = new AnalogConfiguration();

            config.xCenter = parsed[2];
            config.yCenter = parsed[3];
            config.xMax = parsed[0] + config.xCenter;
            config.yMax = parsed[1] + config.yCenter;
            config.xMin = config.xCenter - parsed[4];
            config.yMin = config.yCenter - parsed[5];

            return config;
        }
    }
}

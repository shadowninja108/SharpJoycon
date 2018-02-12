using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;

namespace Joycon_Glue.Source.Joystick
{
    class RightJoycon : NintendoController 
    {
        private Buttons buttons;
        private StickPos pos;
        private AnalogConfiguration stickConfig;

        public RightJoycon(SimplifiedHidDevice device) : base(device)
        {

        }

        public override Buttons GetButtons()
        {
            return buttons;
        }

        public override POVDirection GetPov()
        {
            return POVDirection.None;
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

        public override void Poll(HidDeviceData data) 
        {
            //reset to default values
            buttons = new Buttons();
            byte[] bytes = data.Data;
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
                pos = new StickPos(posX, posY);
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

        public override AnalogConfiguration GetAnalogConfiguration(ConfigurationType type)
        {
            byte[] data = ReadSPI(0x6046, 0x604E - 0x6046);

            int[] parsed = ParseAnalogConfiguration(data);

            AnalogConfiguration config = new AnalogConfiguration();

            config.xMax = parsed[0];
            config.yMax = parsed[1];
            config.xCenter = parsed[2];
            config.yCenter = parsed[3];
            config.xMin = parsed[4];
            config.yMin = parsed[5];

            return config;
        }
    }
}

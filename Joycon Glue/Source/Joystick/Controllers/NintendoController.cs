using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using HidLibrary;
using Joycon_Glue;

//joycon m e n
public abstract class NintendoController : InputJoystick
{
    private byte packetNumber = 0;

    public NintendoController(SimplifiedHidDevice hid) : base(hid)
    {
        
    }

    private void IncreasePacketNumber()
    {
        packetNumber++;
        if (packetNumber < 0xF)
            packetNumber = 0;
    }

    public void SetPlayerLights(PlayerLightState state)
    {
        byte[] data = new byte[2];
        switch (state)
        {
            // empty requires all zeros, so it will default to that anyway!
            case PlayerLightState.Player1:
                data[0] = 1;
                break;
            case PlayerLightState.Player2:
                data[0] = 2;
                break;
            case PlayerLightState.Player3:
                data[0] = 4;
                break;
            case PlayerLightState.Player4:
                data[0] = 8;
                break;
            case PlayerLightState.Keep:
                data[0] = 200;
                data[1] = 10;
                break;
        }
        SendSubcommand(0x1, 0x30, data);
    }

    public void SetVibration(bool enable)
    {
        SendSubcommand(0x1, 0x48, new byte[] { Convert.ToByte(enable) });
    }

    public void SetIMU(bool enable)
    {
        SendSubcommand(0x01, 0x40, new byte[] { Convert.ToByte(enable) });
    }

    // will change to enum when all modes are documented
    public void SetReportMode(byte mode)
    {
        SendSubcommand(0x01, 0x03, new byte[] { mode });
    }

    public byte[] ReadSPI(UInt32 address, byte length) {
        List<byte> outputBytes = new List<byte>(BitConverter.GetBytes(address));
        outputBytes.Add(length);
        byte[] output = outputBytes.ToArray();
        // no idea why i need to constantly send the request but it works
        while (true)
        {
            SendSubcommand(0x1, 0x10, outputBytes.ToArray());
            byte[] input = GetPacketData();
            byte[] header = new byte[outputBytes.Count];
            Array.Copy(input, 15, header, 0, outputBytes.Count);
            if (header.SequenceEqual(output))
            {
                byte[] read = new byte[length];
                Array.Copy(input, 15 + header.Length, read, 0, length);
                return read;
            }
        }
    }

    public int[] ParseAnalogConfiguration(byte[] data)
    {
        int[] config = new int[6];
        config[0] = (data[1] << 8) & 0xF00 | data[0];
        config[1] = (data[2] << 4) | (data[1] >> 4);
        config[2] = (data[4] << 8) & 0xF00 | data[3];
        config[3] = (data[5] << 4) | (data[4] >> 4);
        config[4] = (data[7] << 8) & 0xF00 | data[6];
        config[5] = (data[8] << 4) | (data[7] >> 4);

        return config;
    }

    public abstract AnalogConfiguration GetAnalogConfiguration(ConfigurationType type);

    public Color getBodyColor()
    {
        byte[] bytes = ReadSPI(0x6050, 0x3);
        return Color.FromRgb(bytes[0], bytes[1], bytes[2]);
    }

    public void SendCommand(byte command, byte[] data, int len) {
        byte[] output = new byte[0x40];
        output[0] = command;
        Array.Copy(data, 0, output, 1, len);
        hid.GetHidDevice().Write(output);
    }

    public void SendSubcommand(byte command, byte subCommand, byte[] data)
    {
        byte[] output = new byte[0x40];
        output[0] = packetNumber;
        // rumble data (unimplemented)
        output[9] = subCommand;
        Array.Copy(data, 0, output, 10, data.Length);
        SendCommand(command, output, 10 + data.Length);
        IncreasePacketNumber();
    }

    public byte[] GetPacketData()
    {
        byte[] data = GetHidDevice().GetHidDevice().Read().Data;
        /*if(data.Length > 0) { 
            int offset = 15;
            int size = data.Length - offset;
            size = Math.Max(size, 0);
            byte[] packet = new byte[size];
            Array.Copy(data, offset, packet, 0, size);
            return packet;
        }*/
        return data;
    }

    public override bool GetButton(int id)
    {
        Buttons buttons = GetButtons();
        // should follow xbox controller "standard"
        switch (id)
        {
            case 1:
                return buttons.A;
            case 2:
                return buttons.B;
            case 3:
                return buttons.X;
            case 4:
                return buttons.Y;
            case 5:
                return buttons.ZL;
            case 6:
                return buttons.ZR;
            case 7:
                return buttons.minus;
            case 8:
                return buttons.plus;
            case 9:
                return buttons.stickL;
            case 10:
                return buttons.stickR;
            case 11:
                return buttons.L;
            case 12:
                return buttons.R;
            case 13:
                return buttons.home;
            case 14:
                return buttons.SL;
            case 15:
                return buttons.SR;
            case 16:
                return buttons.capture;
            default:
                return false;
        }
    }

    public override int ButtonCount()
    {
        return 16;
    }

    public abstract Buttons GetButtons();

    public SimplifiedHidDevice GetHidDevice()
    {
        return hid;
    }

    public struct Buttons
    {
        public bool povUp, povDown, povLeft, povRight, stickL, stickR, capture, home, minus, plus, A, B, X, Y, L, R, ZL, ZR, SL, SR;
    }

    public struct AnalogConfiguration
    {
        public int xMax, yMax, xCenter, yCenter, xMin, yMin;
    }

    public enum ControllerType
    {
        LeftJoycon, RightJoycon, ProController, Unknown
    }

    public enum PlayerLightState
    {
        Empty, Player1, Player2, Player3, Player4, Keep
    }

    public enum ConfigurationType
    {
        Factory, User
    }

}

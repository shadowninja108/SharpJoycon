using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using HidLibrary;
using Joycon_Glue;
using Joycon_Glue.Source.Joystick.Controllers;
using Joycon_Glue.Source.Joystick.Controllers.Interfaces;

//joycon m e n
public class NintendoController
{
    private HidDevice device;

    private HIDInterface hid;
    private CommandInterface command;
    private SPIInterface spi;
    private ConfigurationInterface config;
    private HardwareInterface hardware;
    private ControllerInterface controller;

    public NintendoController(HidDevice device)
    {
        this.device = device;
    }

    public HidDevice GetHid() => device;

    public HIDInterface GetHID()
    {
        hid = hid ?? new HIDInterface(this);
        return hid;
    }

    public SPIInterface GetSPI()
    {
        spi = spi ?? new SPIInterface(this);
        return spi;
    }

    public CommandInterface GetCommands()
    {
        command = command ?? new CommandInterface(this);
        return command;
    }

    public ConfigurationInterface GetConfig()
    {
        config = config ?? new ConfigurationInterface(this);
        return config;
    }

    public ControllerInterface GetController()
    {
        controller = controller ?? new ControllerInterface(this);
        return controller;
    }

    public HardwareInterface GetHardware()
    {
        hardware = hardware ?? new HardwareInterface(this);
        return hardware;
    }

    public void Poll()
    {
        HIDInterface.PacketData data = GetHID().ReadPacket();
        GetHID().Poll(data);
        GetSPI().Poll(data);
        GetCommands().Poll(data);
        GetConfig().Poll(data);
        GetController().Poll(data);
        GetHardware().Poll(data);
    }

    

    /*public byte[] ReadSPI(UInt32 address, byte length) {
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
    }*/
}

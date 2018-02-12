using System;

using HidLibrary;

//pretty much a wrapper to make things easier
public class SimplifiedHidDevice
{

    private HidDevice device;

	public SimplifiedHidDevice(HidDevice device)
	{
        this.device = device;
	}

    public String GetSerialNumber()
    {
        byte[] bytes;
        device.ReadSerialNumber(out bytes);
        return BytesToString(bytes);
    }

    public string GetProductString()
    {
        byte[] bytes;
        device.ReadProduct(out bytes);
        return BytesToString(bytes);
    }

    public string GetManufacturerString()
    {
        byte[] bytes;
        device.ReadManufacturer(out bytes);
        return BytesToString(bytes);
    }

    private string BytesToString(byte[] input)
    {
        string str = "";
        foreach (byte b in input)
        {
            if (b > 0)
                str += ((char)b).ToString();
        }
        return str;
    }

    public HidDeviceAttributes GetAttributes()
    {
        return device.Attributes;
    }

    public HidDevice GetHidDevice()
    {
        return device;
    }
}

namespace KYC.API.Proxy.Models;

public class InputData
{
    public const string ZeroAddress = "0x0000000000000000000000000000000000000000";
    public string Address { get; set; } = null!;
    public bool Valid => !string.IsNullOrWhiteSpace(Address) && Address != ZeroAddress;
}
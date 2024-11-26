using Net.Web3.EthereumWallet;
using Net.Web3.EthereumWallet.Extensions;

namespace KYC.API.Proxy.Models;

public class InputData
{
    public string Address { get; set; } = null!;
    public bool Valid => !string.IsNullOrWhiteSpace(Address) && Address != EthereumAddress.ZeroAddress && Address.IsValidEthereumAddressHexFormat();
}
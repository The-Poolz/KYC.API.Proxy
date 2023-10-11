using KYC.DataBase.Models;

namespace KycWebHook.Models;

public class HttpResponse : User
{
    public string Event { get; set; } = null!;
}
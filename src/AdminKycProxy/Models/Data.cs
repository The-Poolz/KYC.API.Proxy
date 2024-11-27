using KYC.DataBase.Models;

namespace AdminKycProxy.Models;

public class Data
{
    public User[] Records { get; set; } = [];
    public int Total { get; set; }
    public int Skip { get; set; }
    public int Limit { get; set; }
}
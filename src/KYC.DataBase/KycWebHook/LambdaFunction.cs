using System.Net;
using KYC.DataBase;
using KycWebHook.Models;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace KycWebHook;

public class LambdaFunction
{
    private readonly KycDbContext context;

    public LambdaFunction()
        : this(new KycDbContext())
    { }

    public LambdaFunction(KycDbContext context)
    {
        this.context = context;
    }

    public async Task<int> Run(HttpResponse httpResponse)
    {
        Console.WriteLine(JToken.FromObject(httpResponse));

        var user = context.Users.FirstOrDefault(x => x.RecordId == httpResponse.RecordId);
        if (user == null)
        {
            context.Users.Add(httpResponse);
        }
        else
        {
            context.Users.Update(httpResponse);
        }
        await context.SaveChangesAsync();

        return (int)HttpStatusCode.OK;
    }
}
using System.Text;
using KYC.DataBase;
using Newtonsoft.Json;
using KycWebHook.Models;
using Amazon.Lambda.Core;
using System.Security.Cryptography;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.RegularExpressions;

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

    public async Task<APIGatewayProxyResponse> RunAsync(APIGatewayProxyRequest request)
    {
        Console.WriteLine(JsonConvert.SerializeObject(request));

        if (!request.Headers.ContainsKey("X-Hub-Signature"))
        {
            return Responses.MissingSignature;
        }

        var actualSignature = StringToSha256(Regex.Unescape(request.Body));
        var signature = request.Headers["X-Hub-Signature"];

        Console.WriteLine($"Actual Signature: {actualSignature}");
        Console.WriteLine($"Signature: {signature}");

        if (actualSignature != signature)
        {
            return Responses.InvalidSignature;
        }

        var httpResponse = JsonConvert.DeserializeObject<HttpResponse>(request.Body)!;

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

        return Responses.OkResponse;
    }

    public static string StringToSha256(string str)
    {
        using var sha256Hash = SHA256.Create();
        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(str));

        var builder = new StringBuilder();
        foreach (var t in bytes)
        {
            builder.Append(t.ToString("x2"));
        }
        return builder.ToString();
    }
}
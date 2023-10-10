using System.Net;
using Amazon.Lambda.Core;
using KycWebHook.Models;
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace KycWebHook;

public class LambdaFunction
{
    public int Run(HttpResponse httpResponse)
    {
        // save into db

        Console.WriteLine(JToken.FromObject(httpResponse));

        return (int)HttpStatusCode.OK;
    }
}
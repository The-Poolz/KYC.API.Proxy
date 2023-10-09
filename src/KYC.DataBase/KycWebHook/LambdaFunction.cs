using System.Net;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace KycWebHook;

public class LambdaFunction
{
    public int Run(JObject input)
    {
        // save into db

        return (int)HttpStatusCode.OK;
    }
}
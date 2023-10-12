using System.Net;
using Amazon.Lambda.APIGatewayEvents;

namespace KycWebHook;

public static class Responses
{
    public static APIGatewayProxyResponse MissingSignature => new()
    {
        StatusCode = (int)HttpStatusCode.BadRequest,
        Headers = new Dictionary<string, string>
        {
            { "Content-Type", "text/plain" }
        },
        Body = "Signature is missing."
    };

    public static APIGatewayProxyResponse InvalidSignature => new()
    {
        StatusCode = (int)HttpStatusCode.BadRequest,
        Headers = new Dictionary<string, string>
        {
            { "Content-Type", "text/plain" }
        },
        Body = "Invalid signature."
    };

    public static APIGatewayProxyResponse OkResponse => new()
    {
        StatusCode = (int)HttpStatusCode.OK
    };
}
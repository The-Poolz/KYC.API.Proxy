using Xunit;
using System.Net;
using KYC.DataBase;
using Newtonsoft.Json;
using FluentAssertions;
using KycWebHook.Models;
using Amazon.Lambda.APIGatewayEvents;
using ConfiguredSqlConnection.Extensions;

namespace KycWebHook.Tests;

public class LambdaFunctionTests
{
    private readonly HttpResponse httpResponse;

    private readonly APIGatewayProxyRequest request;

    public LambdaFunctionTests()
    {
        httpResponse = new HttpResponse
        {
            Guid = "614967cde3227d00125ebc4f",
            Status = "deleted",
            ClientId = "client_id",
            Event = "user.deleted",
            RecordId = "5ffffb44baaaaf001236b1d1",
            RefId = null
        };

        var strHttpResponse = JsonConvert.SerializeObject(httpResponse);
        request = new APIGatewayProxyRequest
        {
            Headers = new Dictionary<string, string>
            {
                { "X-Hub-Signature", LambdaFunction.StringToSha256(strHttpResponse) }
            },
            Body = strHttpResponse
        };
    }

    [Fact]
    public async Task RunAsync_WhenUserNotExist()
    {
        var context = new DbContextFactory<KycDbContext>().Create(ContextOption.InMemory, Guid.NewGuid().ToString());

        var result = await new LambdaFunction(context).RunAsync(request);

        result.Should().BeEquivalentTo(Responses.OkResponse);
        context.Users.Should().HaveCount(1);
    }

    [Fact]
    public async Task RunAsync_WhenUserExist()
    {
        var context = new DbContextFactory<KycDbContext>().Create(ContextOption.InMemory, Guid.NewGuid().ToString());
        context.Users.Add(httpResponse);
        await context.SaveChangesAsync();

        var result = await new LambdaFunction(context).RunAsync(request);

        result.Should().BeEquivalentTo(Responses.OkResponse);
        context.Users.First().Should().BeEquivalentTo(httpResponse);
    }
}

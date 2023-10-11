using ConfiguredSqlConnection.Extensions;
using Xunit;
using FluentAssertions;
using KYC.DataBase;
using KycWebHook.Models;
using Moq;
using System.Net;

namespace KycWebHook.Tests;

public class LambdaFunctionTests
{
    private readonly HttpResponse httpResponse = new()
    {
        Guid = "614967cde3227d00125ebc4f",
        Status = "deleted",
        ClientId = "client_id",
        Event = "user.deleted",
        RecordId = "5ffffb44baaaaf001236b1d1",
        RefId = null
        
    };

    [Fact]
    public async Task RunAsync_WhenUserNotExist()
    {
        var context = new DbContextFactory<KycDbContext>().Create(ContextOption.InMemory, Guid.NewGuid().ToString());

        var result = await new LambdaFunction(context).RunAsync(httpResponse);

        result.Should().Be((int)HttpStatusCode.OK);
        context.Users.Should().HaveCount(1);
    }

    [Fact]
    public async Task RunAsync_WhenUserExist()
    {
        var updatedUser = new HttpResponse
        {
            Guid = "614967cde3227d00125ebc4f",
            Status = "blocked",
            ClientId = "client_id",
            Event = "user.deleted",
            RecordId = "5ffffb44baaaaf001236b1d1",
            RefId = null
        };
        var context = new DbContextFactory<KycDbContext>().Create(ContextOption.InMemory, Guid.NewGuid().ToString());
        context.Users.Add(httpResponse);
        await context.SaveChangesAsync();

        var result = await new LambdaFunction(context).RunAsync(updatedUser);

        result.Should().Be((int)HttpStatusCode.OK);
        context.Users.First().Should().BeEquivalentTo(updatedUser);
    }
}

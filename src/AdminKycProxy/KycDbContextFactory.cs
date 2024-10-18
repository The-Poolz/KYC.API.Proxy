using KYC.DataBase;
using Microsoft.EntityFrameworkCore;
using ConfiguredSqlConnection.Extensions;

namespace AdminKycProxy;

public class KycDbContextFactory : IDbContextFactory<KycDbContext>
{
    public KycDbContext CreateDbContext()
    {
        return new DbContextFactory<KycDbContext>().Create(ContextOption.Staging, "Stage");
    }
}
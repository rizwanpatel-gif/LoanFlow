using LoanFlow.API.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanFlow.Tests;

public static class TestDbHelper
{
    public static LoanFlowDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LoanFlowDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new LoanFlowDbContext(options);
    }
}

using LoanFlow.API.DTOs;
using LoanFlow.API.Services;

namespace LoanFlow.Tests;

public class BorrowerServiceTests
{
    private readonly BorrowerService _service;

    public BorrowerServiceTests()
    {
        var db = TestDbHelper.CreateContext();
        _service = new BorrowerService(db);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBorrower()
    {
        var request = new CreateBorrowerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "5551234567",
            Ssn = "123456789",
            DateOfBirth = new DateOnly(1990, 5, 15),
            Address = "123 Main St",
            AnnualIncome = 75000,
            EmploymentStatus = "Employed"
        };

        var result = await _service.CreateAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal(75000, result.AnnualIncome);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingBorrower_ShouldReturn()
    {
        var request = new CreateBorrowerRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Phone = "5559876543",
            Ssn = "987654321",
            DateOfBirth = new DateOnly(1985, 3, 20),
            Address = "456 Oak Ave",
            AnnualIncome = 90000,
            EmploymentStatus = "Employed"
        };

        var created = await _service.CreateAsync(request);
        var result = await _service.GetByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("Smith", result.LastName);
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ShouldReturnNull()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields()
    {
        var request = new CreateBorrowerRequest
        {
            FirstName = "Bob",
            LastName = "Wilson",
            Email = "bob@example.com",
            Phone = "5551112222",
            Ssn = "111223333",
            DateOfBirth = new DateOnly(1992, 8, 10),
            Address = "789 Pine St",
            AnnualIncome = 60000,
            EmploymentStatus = "Employed"
        };

        var created = await _service.CreateAsync(request);
        var updateRequest = new UpdateBorrowerRequest
        {
            FirstName = "Robert",
            AnnualIncome = 85000
        };

        var result = await _service.UpdateAsync(created.Id, updateRequest);

        Assert.NotNull(result);
        Assert.Equal("Robert", result.FirstName);
        Assert.Equal(85000, result.AnnualIncome);
        Assert.Equal("Wilson", result.LastName);
    }

    [Fact]
    public async Task DeleteAsync_ExistingBorrower_ShouldReturnTrue()
    {
        var request = new CreateBorrowerRequest
        {
            FirstName = "Delete",
            LastName = "Me",
            Email = "delete@example.com",
            Phone = "5553334444",
            Ssn = "444556666",
            DateOfBirth = new DateOnly(1988, 1, 1),
            Address = "999 Elm St",
            AnnualIncome = 50000,
            EmploymentStatus = "Self-Employed"
        };

        var created = await _service.CreateAsync(request);
        var deleted = await _service.DeleteAsync(created.Id);
        var fetched = await _service.GetByIdAsync(created.Id);

        Assert.True(deleted);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginated()
    {
        for (int i = 0; i < 5; i++)
        {
            await _service.CreateAsync(new CreateBorrowerRequest
            {
                FirstName = $"User{i}",
                LastName = "Test",
                Email = $"user{i}@example.com",
                Phone = $"555000{i}000",
                Ssn = $"00000000{i}",
                DateOfBirth = new DateOnly(1990, 1, 1),
                Address = $"{i} Test St",
                AnnualIncome = 50000 + i * 10000,
                EmploymentStatus = "Employed"
            });
        }

        var page1 = await _service.GetAllAsync(1, 3);
        var page2 = await _service.GetAllAsync(2, 3);

        Assert.Equal(3, page1.Count());
        Assert.Equal(2, page2.Count());
    }
}

using FluentAssertions;
using MovieRental.Domain.DTOs.Common;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace MovieRental.Tests.Unit.DTOs;

public class PaginationInputDtoTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(1, 1)]
    [InlineData(5, 5)]
    public void Page_WhenAssigned_NormalizesBelowOneToOne(int input, int expected)
    {
        var dto = new PaginationInputDto { Page = input };
        dto.Page.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-10, 10)]
    [InlineData(1, 1)]
    [InlineData(50, 50)]
    [InlineData(100, 100)]
    [InlineData(101, 100)]
    [InlineData(5000, 100)]
    public void PageSize_WhenAssigned_NormalizesBetweenOneAndOneHundred(int input, int expected)
    {
        var dto = new PaginationInputDto { PageSize = input };
        dto.PageSize.Should().Be(expected);
    }

    [Fact]
    public void PaginationInputDto_DefaultValues_AreSensible()
    {
        var dto = new PaginationInputDto();
        dto.Page.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.SortOrder.Should().Be("desc");
        dto.Search.Should().BeNull();
        dto.SortBy.Should().BeNull();
    }

    [Fact]
    public void PaginationInputDto_DataAnnotations_ValidateRanges()
    {
        var dto = new PaginationInputDto();
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
        isValid.Should().BeTrue();
    }
}

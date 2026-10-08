using FluentAssertions;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Roles;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class RoleServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly RoleService _sut;

    public RoleServiceTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _sut = new RoleService(_roleRepositoryMock.Object);
    }

    private static List<Role> CreateSampleRoles()
    {
        return new List<Role>
        {
            new() { RoleId = 1, RoleName = "Admin", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new() { RoleId = 2, RoleName = "Staff", CreatedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new() { RoleId = 3, RoleName = "Customer", CreatedAt = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc) }
        };
    }

    // =========================================================
    // GetAllRolesAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAllRolesAsync_WhenRolesExist_ReturnsPaginatedList()
    {
        // Arrange
        var roles = CreateSampleRoles();
        _roleRepositoryMock.Setup(r => r.GetAllRoles()).Returns(roles.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new RoleFilterDto();

        // Act
        var result = await _sut.GetAllRolesAsync(pagination, filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.TotalPages.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllRolesAsync_WithSearchTerm_FiltersByRoleName()
    {
        // Arrange
        var roles = CreateSampleRoles();
        _roleRepositoryMock.Setup(r => r.GetAllRoles()).Returns(roles.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "adm" };
        var filter = new RoleFilterDto();

        // Act
        var result = await _sut.GetAllRolesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(r => r.RoleName == "Admin");
    }

    [Fact]
    public async Task GetAllRolesAsync_WithRoleIdFilter_ReturnsOnlyMatchingRole()
    {
        // Arrange
        var roles = CreateSampleRoles();
        _roleRepositoryMock.Setup(r => r.GetAllRoles()).Returns(roles.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new RoleFilterDto { RoleId = 2 };

        // Act
        var result = await _sut.GetAllRolesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(r => r.RoleId == 2 && r.RoleName == "Staff");
    }

    [Theory]
    [InlineData("rolename", "asc", "Admin")]
    [InlineData("rolename", "desc", "Staff")]
    [InlineData("default", "desc", "Customer")]
    public async Task GetAllRolesAsync_WithSorting_OrdersResultsCorrectly(string sortBy, string sortOrder, string expectedFirstRoleName)
    {
        // Arrange
        var roles = CreateSampleRoles();
        _roleRepositoryMock.Setup(r => r.GetAllRoles()).Returns(roles.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var filter = new RoleFilterDto();

        // Act
        var result = await _sut.GetAllRolesAsync(pagination, filter);

        // Assert
        result.Data.First().RoleName.Should().Be(expectedFirstRoleName);
    }

    [Fact]
    public async Task GetAllRolesAsync_WhenEmpty_ReturnsZeroTotals()
    {
        // Arrange
        var emptyList = new List<Role>();
        _roleRepositoryMock.Setup(r => r.GetAllRoles()).Returns(emptyList.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new RoleFilterDto();

        // Act
        var result = await _sut.GetAllRolesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    // =========================================================
    // CreateRoleAsync Tests
    // =========================================================

    [Fact]
    public async Task CreateRoleAsync_WhenRoleDoesNotExist_CreatesAndReturnsRole()
    {
        // Arrange
        var dto = new CreateRoleDto { RoleName = "Manager" };

        _roleRepositoryMock.Setup(r => r.RoleExistsAsync("Manager"))
            .ReturnsAsync(false);

        _roleRepositoryMock.Setup(r => r.CreateRoleAsync(It.IsAny<Role>()))
            .ReturnsAsync((Role role) =>
            {
                role.RoleId = 4;
                return role;
            });

        // Act
        var result = await _sut.CreateRoleAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.RoleId.Should().Be(4);
        result.RoleName.Should().Be("Manager");

        _roleRepositoryMock.Verify(r => r.RoleExistsAsync("Manager"), Times.Once);
        _roleRepositoryMock.Verify(r => r.CreateRoleAsync(It.Is<Role>(role => role.RoleName == "Manager")), Times.Once);
    }

    [Fact]
    public async Task CreateRoleAsync_WhenRoleAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateRoleDto { RoleName = "Admin" };

        _roleRepositoryMock.Setup(r => r.RoleExistsAsync("Admin"))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.CreateRoleAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Role already exists");

        _roleRepositoryMock.Verify(r => r.CreateRoleAsync(It.IsAny<Role>()), Times.Never);
    }
}

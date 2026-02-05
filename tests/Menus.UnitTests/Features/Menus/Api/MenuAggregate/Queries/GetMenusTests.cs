namespace Menus.UnitTests.Features.Menus.Api.MenuAggregate.Queries;

public class GetMenusTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly Mock<IQuery> _queryMock;
    private readonly GetMenus.Service _service;

    public GetMenusTests()
    {
        _createMenu = new(_menuValidator);
        _queryMock = new Mock<IQuery>();
        _service = new GetMenus.Service(_queryMock.Object);
    }

    private MenuAgg CreateMenu(
        string name = "Test Menu",
        bool isActive = false,
        int displayOrder = 0)
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: name
        ));

        menu.GetType().GetProperty("IsActive")!.SetValue(menu, isActive);
        menu.GetType().GetProperty("DisplayOrder")!.SetValue(menu, displayOrder);

        return menu;
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithMenus_ReturnsAllMenus()
    {
        var menus = new List<MenuAgg>
        {
            CreateMenu("Menú Degustación"),
            CreateMenu("Menú Ejecutivo")
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<MenuAgg>()).Returns(menus);

        var response = await _service.HandleAsync(null);

        response.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WithNoMenus_ReturnsEmptyList()
    {
        var menus = new List<MenuAgg>().AsAsyncQueryable();
        _queryMock.Setup(q => q.Query<MenuAgg>()).Returns(menus);

        var response = await _service.HandleAsync(null);

        response.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ReturnsCorrectlyMappedResponse()
    {
        var menu = CreateMenu("Menú Navideño");
        var menus = new List<MenuAgg> { menu }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<MenuAgg>()).Returns(menus);

        var response = await _service.HandleAsync(null);

        response.Should().HaveCount(1);
        var result = response.First();
        result.Id.Should().Be(menu.Id);
        result.Name.Should().Be("Menú Navideño");
    }

    [Fact]
    public async Task HandleAsync_OrdersByDisplayOrderThenByName()
    {
        var menus = new List<MenuAgg>
        {
            CreateMenu("C - Menu", displayOrder: 2),
            CreateMenu("B - Menu", displayOrder: 1),
            CreateMenu("A - Menu", displayOrder: 1),
            CreateMenu("D - Menu", displayOrder: 0)
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<MenuAgg>()).Returns(menus);

        var response = await _service.HandleAsync(null);

        response.Should().HaveCount(4);
        response[0].Name.Should().Be("D - Menu");
        response[1].Name.Should().Be("A - Menu");
        response[2].Name.Should().Be("B - Menu");
        response[3].Name.Should().Be("C - Menu");
    }

    #endregion

    #region Filter Tests

    [Fact]
    public async Task HandleAsync_WithIsActiveTrue_ReturnsOnlyActiveMenus()
    {
        var menus = new List<MenuAgg>
        {
            CreateMenu("Menú Activo", isActive: true),
            CreateMenu("Menú Inactivo", isActive: false)
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<MenuAgg>()).Returns(menus);

        var response = await _service.HandleAsync(isActive: true);

        response.Should().HaveCount(1);
        response.First().Name.Should().Be("Menú Activo");
        response.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithIsActiveFalse_ReturnsOnlyInactiveMenus()
    {
        var menus = new List<MenuAgg>
        {
            CreateMenu("Menú Activo", isActive: true),
            CreateMenu("Menú Inactivo", isActive: false)
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<MenuAgg>()).Returns(menus);

        var response = await _service.HandleAsync(isActive: false);

        response.Should().HaveCount(1);
        response.First().Name.Should().Be("Menú Inactivo");
        response.First().IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithIsActiveNull_ReturnsAllMenus()
    {
        var menus = new List<MenuAgg>
        {
            CreateMenu("Menú Activo", isActive: true),
            CreateMenu("Menú Inactivo", isActive: false)
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<MenuAgg>()).Returns(menus);

        var response = await _service.HandleAsync(isActive: null);

        response.Should().HaveCount(2);
    }

    #endregion

    #region Query Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsQueryMethod()
    {
        var menus = new List<MenuAgg>().AsAsyncQueryable();
        _queryMock.Setup(q => q.Query<MenuAgg>()).Returns(menus);

        await _service.HandleAsync(null);

        _queryMock.Verify(q => q.Query<MenuAgg>(), Times.Once);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_ReturnsOkResult()
    {
        var serviceMock = new Mock<GetMenus.IService>();
        var expectedResponse = new List<MenuResponse>
        {
            new(Guid.NewGuid(), "Test Menu", null, false, 0, null, null, null, [])
        };

        serviceMock.Setup(s => s.HandleAsync(null)).ReturnsAsync(expectedResponse);

        var result = await GetMenus.Handler(serviceMock.Object, null);

        result.Should().BeOfType<Ok<List<MenuResponse>>>();
        var okResult = (Ok<List<MenuResponse>>)result;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Handler_CallsServiceWithFilter()
    {
        var serviceMock = new Mock<GetMenus.IService>();
        serviceMock.Setup(s => s.HandleAsync(true)).ReturnsAsync([]);

        await GetMenus.Handler(serviceMock.Object, true);

        serviceMock.Verify(s => s.HandleAsync(true), Times.Once);
    }

    [Fact]
    public async Task Handler_CallsServiceWithNullFilter()
    {
        var serviceMock = new Mock<GetMenus.IService>();
        serviceMock.Setup(s => s.HandleAsync(null)).ReturnsAsync([]);

        await GetMenus.Handler(serviceMock.Object, null);

        serviceMock.Verify(s => s.HandleAsync(null), Times.Once);
    }

    #endregion
}

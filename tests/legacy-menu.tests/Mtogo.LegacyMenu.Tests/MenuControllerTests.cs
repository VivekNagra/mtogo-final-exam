using Microsoft.AspNetCore.Mvc;
using Moq;
using Mtogo.LegacyMenu.Api.Controllers;
using Mtogo.LegacyMenu.Api.Models;
using Mtogo.LegacyMenu.Api.Repositories;
using Mtogo.LegacyMenu.Api.Services;

namespace Mtogo.LegacyMenu.Tests;

public class MenuControllerTests
{
    private readonly MenuService _service;
    private readonly MenuController _controller;

    public MenuControllerTests()
    {

        var db = MenuServiceTests.CreateDb();
        var repo = new MenuRepository(db);
        _service = new MenuService(repo);
        _controller = new MenuController(_service);
    }

    [Fact]
    public async Task GetMenu_ReturnsNotFound_WhenRestaurantDoesNotExist()
    {
        var result = await _controller.GetMenu(Guid.NewGuid(), CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }
}
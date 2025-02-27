using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[TestClass]
public class ItemsControllerTests
{

    private Mock<IItemRepository> _mockRepository;
    private ItemsController _controller;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IItemRepository>();
        _controller = new ItemsController(_mockRepository.Object);
    }

    [TestMethod]
    public async Task Get_ShouldReturnListOfItems()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Id = "1", Name = "Item1", Description = "Description1" },
            new Item { Id = "2", Name = "Item2", Description = "Description2" }
        };

        var mockRepo = new Mock<IItemRepository>();
        mockRepo.Setup(repo => repo.GetItems()).ReturnsAsync(items);
        var controller = new ItemsController(mockRepo.Object);

        // Act
        var result = await controller.Get();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Value.Count);
        Assert.AreEqual("Item1", result.Value[0].Name);
    }

    [TestMethod]
    public async Task Get_WithValidId_ShouldReturnItem()
    {
        // Arrange
        var item = new Item { Id = "1", Name = "Item1", Description = "Description1" };

        var mockRepo = new Mock<IItemRepository>();
        mockRepo.Setup(repo => repo.GetItem("1")).ReturnsAsync(item);
        var controller = new ItemsController(mockRepo.Object);

        // Act
        var result = await controller.Get("1");

        // Assert
        var okResult = result.Result as OkObjectResult; // Ensure it's an Ok result
        Assert.IsNotNull(okResult);
        Assert.IsInstanceOfType(okResult.Value, typeof(Item));
        Assert.AreEqual("Item1", ((Item)okResult.Value).Name);
    }


    [TestMethod]
    public async Task Get_WithInvalidId_ShouldReturnNotFound()
    {
        _mockRepository.Setup(repo => repo.GetItem("999")).ReturnsAsync((Item)null);

        var result = await _controller.Get("999");

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task Post_ShouldCreateItem()
    {
        var newItem = new Item { Id = "3", Name = "NewItem", Description = "NewDescription" };

        _mockRepository.Setup(repo => repo.CreateItem(newItem)).ReturnsAsync(newItem);

        var result = await _controller.Post(newItem);

        var actionResult = result.Result as CreatedAtActionResult;
        var returnedItem = actionResult.Value as Item;

        Assert.IsNotNull(returnedItem);
        Assert.AreEqual("NewItem", returnedItem.Name);
        Assert.AreEqual("NewDescription", returnedItem.Description);
    }

    [TestMethod]
    public async Task Put_ShouldUpdateItem()
    {
        var updatedItem = new Item { Id = "1", Name = "UpdatedItem", Description = "UpdatedDescription" };

        _mockRepository.Setup(repo => repo.UpdateItem("1", updatedItem)).Returns(Task.CompletedTask);

        var result = await _controller.Put("1", updatedItem);

        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public async Task Delete_ShouldRemoveItem()
    {
        _mockRepository.Setup(repo => repo.DeleteItem("1")).Returns(Task.CompletedTask);

        var result = await _controller.Delete("1");

        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }
}

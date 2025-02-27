using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

[TestClass]
public class ItemRepositoryTests
{
    private Mock<IMongoCollection<Item>> _mockCollection;
    private Mock<IMongoDatabase> _mockDatabase;
    private Mock<IMongoClient> _mockClient;
    private Mock<MongoDbContext> _mockDbContext;
    private IItemRepository _repository;

    [TestInitialize]
    public void Setup()
    {
        _mockCollection = new Mock<IMongoCollection<Item>>();
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockClient = new Mock<IMongoClient>();
        _mockDbContext = new Mock<MongoDbContext>("mongodb://localhost:27017", "newdb");

        _mockDatabase.Setup(db => db.GetCollection<Item>("Items", null))
                     .Returns(_mockCollection.Object);
        _mockClient.Setup(client => client.GetDatabase(It.IsAny<string>(), null))
                   .Returns(_mockDatabase.Object);

        _mockDbContext.Setup(ctx => ctx.Items)
                      .Returns(_mockCollection.Object);

        _repository = new ItemRepository(_mockDbContext.Object);
    }

    [TestMethod]
    public async Task GetItems_ShouldReturnListOfItems()
    {
        var items = new List<Item>
        {
            new Item { Id = "1", Name = "Item1", Description = "Description1" },
            new Item { Id = "2", Name = "Item2", Description = "Description2" }
        };

        var mockCursor = new Mock<IAsyncCursor<Item>>();
        mockCursor.Setup(_ => _.Current).Returns(items);
        mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
        mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

        _mockCollection
            .Setup(c => c.FindAsync(It.IsAny<FilterDefinition<Item>>(), It.IsAny<FindOptions<Item>>(), default))
            .ReturnsAsync(mockCursor.Object);

        var result = await _repository.GetItems();

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Item1", result[0].Name);
    }

    [TestMethod]
    public async Task CreateItem_ShouldInsertNewItem()
    {
        var newItem = new Item { Id = "3", Name = "NewItem", Description = "NewDescription" };

        _mockCollection
            .Setup(c => c.InsertOneAsync(It.IsAny<Item>(), null, default))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await _repository.CreateItem(newItem);

        Assert.IsNotNull(result);
        Assert.AreEqual("NewItem", result.Name);

        _mockCollection.Verify(c => c.InsertOneAsync(It.Is<Item>(i => i.Id == "3" && i.Name == "NewItem"), null, default), Times.Once);
    }

    [TestMethod]
    public async Task GetItem_ShouldReturnSingleItem()
    {
        var item = new Item { Id = "1", Name = "Item1", Description = "Description1" };

        var mockCursor = new Mock<IAsyncCursor<Item>>();
        mockCursor.Setup(_ => _.Current).Returns(new List<Item> { item });
        mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
        mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

        _mockCollection
            .Setup(c => c.FindAsync(It.IsAny<FilterDefinition<Item>>(), It.IsAny<FindOptions<Item>>(), default))
            .ReturnsAsync(mockCursor.Object);

        var result = await _repository.GetItem("1");

        Assert.IsNotNull(result);
        Assert.AreEqual("1", result.Id);
        Assert.AreEqual("Item1", result.Name);
    }

    [TestMethod]
    public async Task UpdateItem_ShouldReplaceItem()
    {
        var updatedItem = new Item { Id = "1", Name = "UpdatedItem", Description = "UpdatedDescription" };

        _mockCollection
            .Setup(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Item>>(),
                It.IsAny<Item>(),
                (ReplaceOptions?)null,
                default))
            .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null))
            .Verifiable();

        await _repository.UpdateItem("1", updatedItem);

        _mockCollection.Verify(c => c.ReplaceOneAsync(
            It.Is<FilterDefinition<Item>>(f => f != null),
            updatedItem,
            (ReplaceOptions?)null,
            default), Times.Once);
    }

    [TestMethod]
    public async Task DeleteItem_ShouldRemoveItem()
    {
        _mockCollection
            .Setup(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<Item>>(), default))
            .ReturnsAsync(new DeleteResult.Acknowledged(1))
            .Verifiable();

        await _repository.DeleteItem("1");

        _mockCollection.Verify(c => c.DeleteOneAsync(It.Is<FilterDefinition<Item>>(f => f != null), default), Times.Once);
    }
}

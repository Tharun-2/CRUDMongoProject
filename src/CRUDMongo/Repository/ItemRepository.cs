using MongoDB.Driver;

public class ItemRepository : IItemRepository
{
    private readonly MongoDbContext _context;

    public ItemRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<List<Item>> GetItems() => await _context.Items.Find(_ => true).ToListAsync();

    public async Task<Item> GetItem(string id) => await _context.Items.Find(item => item.Id == id).FirstOrDefaultAsync();

    public async Task<Item> CreateItem(Item item)
    {
        await _context.Items.InsertOneAsync(item);
        return item;
    }

    public async Task UpdateItem(string id, Item item) => await _context.Items.ReplaceOneAsync(i => i.Id == id, item);

    public async Task DeleteItem(string id) => await _context.Items.DeleteOneAsync(i => i.Id == id);
}
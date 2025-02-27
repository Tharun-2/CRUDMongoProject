using System.Collections.Generic;
using System.Threading.Tasks;

public interface IItemRepository
{
    Task<List<Item>> GetItems();
    Task<Item> GetItem(string id);
    Task<Item> CreateItem(Item item);
    Task UpdateItem(string id, Item item);
    Task DeleteItem(string id);
}

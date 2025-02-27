using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemRepository _repository;

    public ItemsController(IItemRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Item>>> Get() => await _repository.GetItems();

    [HttpGet("{id}")]
    public async Task<ActionResult<Item>> Get(string id)
    {
        var item = await _repository.GetItem(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Item>> Post([FromBody] Item item)
    {
        await _repository.CreateItem(item);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, Item item)
    {
        await _repository.UpdateItem(id, item);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repository.DeleteItem(id);
        return NoContent();
    }
}
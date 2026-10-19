using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioActivos.Data;
using InventarioActivos.Models;

[ApiController]
[Route("api/inventario")]
public class InventarioController : ControllerBase
{
  private readonly InventarioContext _context;
  public InventarioController(InventarioContext context) { _context = context; }

  [HttpGet]
  public async Task<ActionResult<List<Producto>>> Get()
  {
    return await _context.Productos.ToListAsync();
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<Producto>> GetById(int id)
  {
      var producto = await _context.Productos.FindAsync(id);
      if (producto == null)
      {
          return NotFound();
      }
      return producto;
  }
  [HttpPost]
  public async Task<ActionResult<Producto>> Post([FromBody] Producto Producto)
  {
    _context.Productos.Add(Producto);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetById), new { id = Producto.Id }, Producto);
}
}
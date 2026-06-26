using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaEntityFramework.Data;
using TiendaEntityFramework.DTOs.Producto;
using TiendaEntityFramework.Models;

namespace TiendaEntityFramework.Controllers
{
    [ApiController]
    [Route("api/productos")]
    public class ProductosController : ControllerBase
    {
        private readonly TiendaContext _context;
        public ProductosController(TiendaContext context)
        {
            _context = context;
        }

        // ── GET api/productos ───────────────────────────────────────────── 
        // Soporta filtros opcionales: ?soloActivos=true&categoriaId=2&buscar=camisa 
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool? soloActivos,
            [FromQuery] int? categoriaId,
            [FromQuery] string? buscar)
        {

            // Construimos la consulta de forma incremental — EF no va a la BD todavía 
            var query = _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .AsQueryable();

            // Aplicamos filtros solo si el parámetro viene en la URL 
            if (soloActivos == true)
                query = query.Where(p => p.Activo);

            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId.Value);

            if (!string.IsNullOrWhiteSpace(buscar))
                query = query.Where(p => p.Nombre.Contains(buscar) ||
                                         (p.Descripcion != null && p.Descripcion.Contains(buscar)));

            // Ahora sí va a la BD, proyectando directamente al DTO 
            var productos = await query
                .OrderBy(p => p.Nombre)
                .Select(p => new ProductoResponseDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    Activo = p.Activo,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria!.Nombre
                })
                .ToListAsync();

            return Ok(productos);
        }



        // ── GET api/productos/{id} ──────────────────────────────────────── 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var producto = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Where(p => p.Id == id)
                .Select(p => new ProductoResponseDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    Activo = p.Activo,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria!.Nombre
                })
                .FirstOrDefaultAsync();

            if (producto == null)
                return NotFound(new { mensaje = $"Producto {id} no encontrado." });

            return Ok(producto);
        }



        // ── POST api/productos ──────────────────────────────────────────── 
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductoCreateDTO dto)
        {
            // Verificar que la categoría existe 
            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Id == dto.CategoriaId);

            if (!categoriaExiste)
                return BadRequest(new { mensaje = $"La categoría {dto.CategoriaId} no existe." });

            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                Stock = dto.Stock,
                CategoriaId = dto.CategoriaId,
                Activo = true
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            // Recargar con la categoría para devolver CategoriaNombre 
            await _context.Entry(producto).Reference(p => p.Categoria).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = producto.Id },
                new ProductoResponseDTO
                {
                    Id = producto.Id,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio,
                    Stock = producto.Stock,
                    Activo = producto.Activo,
                    CategoriaId = producto.CategoriaId,
                    CategoriaNombre = producto.Categoria!.Nombre
                });

        }



        // ── PUT api/productos/{id} ──────────────────────────────────────── 
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductoUpdateDTO dto)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound(new { mensaje = $"Producto {id} no encontrado." });

            if (!await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId))
                return BadRequest(new { mensaje = $"La categoría {dto.CategoriaId} no existe." });

            producto.Nombre = dto.Nombre;
            producto.Descripcion = dto.Descripcion;
            producto.Precio = dto.Precio;
            producto.Stock = dto.Stock;
            producto.Activo = dto.Activo;
            producto.CategoriaId = dto.CategoriaId;

            await _context.SaveChangesAsync();
            return NoContent();
        }



        // ── PATCH api/productos/{id}/stock ──────────────────────────────── 
        // Modifica solo el stock (entrada o salida de mercancía) 

        [HttpPatch("{id}/stock")]
        public async Task<IActionResult> ActualizarStock(int id, [FromBody] int cantidad)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound(new { mensaje = $"Producto {id} no encontrado." });

            var nuevoStock = producto.Stock + cantidad;
            if (nuevoStock < 0)
                return BadRequest(new { mensaje = "El stock no puede quedar negativo.", stockActual = producto.Stock });

            producto.Stock = nuevoStock;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Stock actualizado.", stockActual = producto.Stock });
        }



        // ── DELETE api/productos/{id} ───────────────────────────────────── 

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {

            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound(new { mensaje = $"Producto {id} no encontrado." });

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

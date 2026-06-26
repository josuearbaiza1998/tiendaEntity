using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaEntityFramework.Data;
using TiendaEntityFramework.DTOs.Categoria;
using TiendaEntityFramework.Models;

namespace TiendaEntityFramework.Controllers
{
    [ApiController]
    [Route("api/categorias")]
    public class CategoriasController : ControllerBase // ← Heredar de ControllerBase para habilitar Ok(), NotFound(), etc.
    {
        private readonly TiendaContext _context;
        public CategoriasController(TiendaContext context)
        {
            _context = context;
        }

        // ── GET api/categorias ──────────────────────────────────────────── 

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categorias = await _context.Categorias
                .AsNoTracking()
                .Select(c => new CategoriaResponseDTO
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    CantidadProductos = c.Productos.Count
                })
                .ToListAsync();

            return Ok(categorias);
        }



        // ── GET api/categorias/{id} ─────────────────────────────────────── 

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)

        {

            var categoria = await _context.Categorias
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CategoriaResponseDTO
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    CantidadProductos = c.Productos.Count
                })
                .FirstOrDefaultAsync();


            if (categoria == null)
                return NotFound(new { mensaje = $"Categoría {id} no encontrada." });



            return Ok(categoria);

        }



        // ── POST api/categorias ─────────────────────────────────────────── 

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoriaCreateDTO dto)
        {
            var categoria = new Categoria
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            var respuesta = new CategoriaResponseDTO
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                CantidadProductos = 0
            };

            return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, respuesta);

        }



        // ── PUT api/categorias/{id} ─────────────────────────────────────── 

        [HttpPut("{id}")]

        public async Task<IActionResult> Update(int id, [FromBody] CategoriaCreateDTO dto)

        {

            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
                return NotFound(new { mensaje = $"Categoría {id} no encontrada." });

            categoria.Nombre = dto.Nombre;
            categoria.Descripcion = dto.Descripcion;

            await _context.SaveChangesAsync();

            return NoContent();
        }



        // ── DELETE api/categorias/{id} ──────────────────────────────────── 

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
                return NotFound(new { mensaje = $"Categoría {id} no encontrada." });

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
            return NoContent();

        }
    }
}

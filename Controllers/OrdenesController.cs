using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaEntityFramework.Data;
using TiendaEntityFramework.DTOs.Orden;
using TiendaEntityFramework.Models;

namespace TiendaEntityFramework.Controllers
{
    [ApiController]
    [Route("api/ordenes")]
    public class OrdenesController : ControllerBase
    {
        private readonly TiendaContext _context;

        public OrdenesController(TiendaContext context)
        {
            _context = context;
        }

        // ── GET api/ordenes ─────────────────────────────────────────────── 

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ordenes = await _context.Ordenes
                .AsNoTracking()
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Producto)   // Detalles.Producto 
                .OrderByDescending(o => o.Fecha)
                .Select(o => MapToResponseDTO(o))
                .ToListAsync();

            return Ok(ordenes);
        }

        // ── GET api/ordenes/{id} ────────────────────────────────────────── 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var orden = await _context.Ordenes
                .AsNoTracking()
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null)
                return NotFound(new { mensaje = $"Orden {id} no encontrada." });

            return Ok(MapToResponseDTO(orden));
        }



        // ── GET api/ordenes/cliente/{nombre} ────────────────────────────── 
        [HttpGet("cliente/{nombre}")]
        public async Task<IActionResult> GetByCliente(string nombre)
        {
            var ordenes = await _context.Ordenes
                .AsNoTracking()
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(o => o.ClienteNombre.Contains(nombre))
                .OrderByDescending(o => o.Fecha)
                .Select(o => MapToResponseDTO(o))
                .ToListAsync();

            return Ok(ordenes);
        }



        // ── POST api/ordenes ────────────────────────────────────────────── 
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrdenCreateDTO dto)
        {
            if (dto.Detalles == null || dto.Detalles.Count == 0)
                return BadRequest(new { mensaje = "La orden debe tener al menos un producto." });

            // ── Abrimos una transacción ──────────────────────────────── 
            // Todo lo que pase dentro del using se confirma (CommitAsync) 
            // o se revierte (RollbackAsync) si algo falla 
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var orden = new Orden
                {
                    ClienteNombre = dto.ClienteNombre,
                    Fecha = DateTime.UtcNow,
                    Detalles = new List<DetalleOrden>()
                };

                decimal totalCalculado = 0;

                foreach (var item in dto.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId);

                    if (producto == null)
                        return BadRequest(new { mensaje = $"Producto {item.ProductoId} no existe." });

                    if (!producto.Activo)
                        return BadRequest(new { mensaje = $"'{producto.Nombre}' no está disponible." });

                    if (producto.Stock < item.Cantidad)
                        return BadRequest(new
                        {
                            mensaje = $"Stock insuficiente para '{producto.Nombre}'.",
                            disponible = producto.Stock
                        });

                    // El precio se toma de la BD, no del cliente 
                    orden.Detalles.Add(new DetalleOrden
                    {
                        ProductoId = producto.Id,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.Precio
                    });
                    totalCalculado += item.Cantidad * producto.Precio;
                    // Descontamos el stock 
                    producto.Stock -= item.Cantidad;
                }

                orden.Total = totalCalculado;

                _context.Ordenes.Add(orden);
                await _context.SaveChangesAsync();  // guarda orden + detalles + stock actualizado 

                await transaction.CommitAsync();    // confirma todo en la BD 

                // Recargar con relaciones para armar la respuesta 
                var ordenCompleta = await _context.Ordenes
                    .Include(o => o.Detalles).ThenInclude(d => d.Producto)
                    .FirstAsync(o => o.Id == orden.Id);

                return CreatedAtAction(nameof(GetById), new { id = orden.Id },
                    MapToResponseDTO(ordenCompleta));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();  // revierte todo 
                return StatusCode(500, new { mensaje = "Error al crear la orden.", detalle = ex.Message });
            }
        }



        // ── DELETE api/ordenes/{id} ─────────────────────────────────────── 
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var orden = await _context.Ordenes
                .Include(o => o.Detalles)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null)
                return NotFound(new { mensaje = $"Orden {id} no encontrada." });

            // Cascade delete: EF borra automáticamente los detalles 
            // porque configuramos DeleteBehavior.Cascade en el DbContext 
            _context.Ordenes.Remove(orden);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ── Método privado para mapear Orden → OrdenResponseDTO ─────────── 
        private static OrdenResponseDTO MapToResponseDTO(Orden o) => new()
        {
            Id = o.Id,
            ClienteNombre = o.ClienteNombre,
            Fecha = o.Fecha,
            Total = o.Total,
            Detalles = o.Detalles.Select(d => new DetalleOrdenResponseDTO
            {
                ProductoId = d.ProductoId,
                ProductoNombre = d.Producto?.Nombre ?? string.Empty,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario
            }).ToList()
        };


    }
}

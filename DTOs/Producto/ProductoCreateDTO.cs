namespace TiendaEntityFramework.DTOs.Producto
{
    public class ProductoCreateDTO
    {
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public int CategoriaId { get; set; }
    }
}

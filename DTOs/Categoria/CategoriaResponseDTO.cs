namespace TiendaEntityFramework.DTOs.Categoria
{
    public class CategoriaResponseDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int CantidadProductos { get; set; }  // dato calculado, no existe como columna 
    }
}

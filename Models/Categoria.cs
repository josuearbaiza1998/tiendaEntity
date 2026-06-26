namespace TiendaEntityFramework.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }  // el ? significa que puede ser null en la BD 



        // Propiedad de navegación: una categoría tiene muchos productos 
        // EF Core la usa para generar los JOINs automáticamente 

        public List<Producto> Productos { get; set; } = new();
    }
}

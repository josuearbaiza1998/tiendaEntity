namespace TiendaEntityFramework.Models
{
    public class Producto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public bool Activo { get; set; } = true;



        // Clave foránea: el Id de la categoría a la que pertenece 

        public int CategoriaId { get; set; }



        // Propiedad de navegación: el objeto Categoria completo 

        // EF Core usa CategoriaId para saber cómo hacer el JOIN 

        public Categoria? Categoria { get; set; }



        // Navegación inversa: este producto puede estar en muchos detalles de orden 

        public List<DetalleOrden> DetallesOrden { get; set; } = new();
    }
}

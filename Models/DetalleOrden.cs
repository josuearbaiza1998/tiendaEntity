namespace TiendaEntityFramework.Models
{
    public class DetalleOrden
    {
        public int Id { get; set; }



        // FK y navegación hacia Orden 

        public int OrdenId { get; set; }

        public Orden? Orden { get; set; }



        // FK y navegación hacia Producto 

        public int ProductoId { get; set; }

        public Producto? Producto { get; set; }



        // Datos propios de este detalle 

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }  // precio al momento de la compra 
    }
}

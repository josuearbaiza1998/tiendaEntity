namespace TiendaEntityFramework.Models
{
    public class Orden
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public string ClienteNombre { get; set; } = string.Empty;

        public decimal Total { get; set; }  // se calcula al crear la orden 



        // Una orden tiene muchos detalles (líneas de la factura) 

        public List<DetalleOrden> Detalles { get; set; } = new();
    }
}

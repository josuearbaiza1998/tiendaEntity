namespace TiendaEntityFramework.DTOs.Orden
{
    public class OrdenResponseDTO
    {
        public int Id { get; set; }

        public string ClienteNombre { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public List<DetalleOrdenResponseDTO> Detalles { get; set; } = new();
    }
}

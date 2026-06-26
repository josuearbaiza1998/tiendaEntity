namespace TiendaEntityFramework.DTOs.Orden
{
    public class OrdenCreateDTO
    {
        public string ClienteNombre { get; set; } = string.Empty;

        public List<DetalleOrdenCreateDTO> Detalles { get; set; } = new();
    }
}

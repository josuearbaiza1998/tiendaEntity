namespace TiendaEntityFramework.DTOs.Orden
{
    public class DetalleOrdenResponseDTO
    {
        public int ProductoId { get; set; }

        public string ProductoNombre { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal => Cantidad * PrecioUnitario;  // calculado, no en BD 
    }
}

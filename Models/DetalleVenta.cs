using System.Text.Json.Serialization;

namespace AmperImp.Models
{
    public class DetalleVenta
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public string Comprador {  get; set; }
        public CategoriaProducto Categoria { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Talle Talle { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => PrecioUnitario * (decimal)Cantidad;
    }
}

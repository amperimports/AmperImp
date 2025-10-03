namespace AmperImp.Models
{
    public class Venta
    {
        public int Id { get; set; } // autoincremental
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Comprador { get; set; }
        public List<DetalleVenta> Detalles { get; set; } = new();
    }
}

namespace AmperImp.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public CategoriaProducto Categoria { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal Costo { get; set; }
        //public string Imagen {  get; set; }
        public List<StockTalle> StockTalles { get; set; } = new List<StockTalle>();
    }
}

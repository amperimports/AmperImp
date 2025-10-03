namespace AmperImp.Services
{
    public static class IdGenerator
    {
        private static int _ultimoIdProducto = 0;
        private static int _ultimoIdVenta = 0;

        public static int NuevoIdProducto() => ++_ultimoIdProducto;
        public static int NuevoIdVenta() => ++_ultimoIdVenta;
    }
}

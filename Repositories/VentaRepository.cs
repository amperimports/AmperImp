using AmperImp.Models;
using AmperImp.Services;

namespace AmperImp.Repositories
{
    public class VentaRepository
    {
        private readonly JsonRepository<Venta> _jsonRepo;

        public VentaRepository(string rutaArchivo)
        {
            _jsonRepo = new JsonRepository<Venta>(rutaArchivo);

            // Ajustamos el IdGenerator al máximo ID existente en el JSON
            var ventas = _jsonRepo.GetAll();
            if (ventas.Any())
            {
                int maxId = ventas.Max(v => v.Id);
                // "inyectamos" el valor en el IdGenerator
                for (int i = 0; i < maxId; i++)
                    IdGenerator.NuevoIdVenta();
            }
        }

        public List<Venta> GetAll() => _jsonRepo.GetAll();

        public Venta? GetById(int id)
        {
            return _jsonRepo.GetById(v => v.Id == id);
        }

        public Venta Add(Venta venta)
        {
            venta.Id = IdGenerator.NuevoIdProducto();
            _jsonRepo.Add(venta);
            return venta;
        }

        public void Update(Venta venta)
        {
            _jsonRepo.UpdateItem(venta, v => v.Id == venta.Id);
        }

        public void Delete(int id)
        {
            _jsonRepo.Delete(p => p.Id == id);
        }
    }
}

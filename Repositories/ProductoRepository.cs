using AmperImp.Models;
using AmperImp.Services;

namespace AmperImp.Repositories
{
    public class ProductoRepository
    {
        private readonly JsonRepository<Producto> _jsonRepo;

        public ProductoRepository(string rutaArchivo)
        {
            _jsonRepo = new JsonRepository<Producto>(rutaArchivo);

            // Ajustamos el IdGenerator al máximo ID existente en el JSON
            var productos = _jsonRepo.GetAll();
            if (productos.Any())
            {
                int maxId = productos.Max(p => p.Id);
                // "inyectamos" el valor en el IdGenerator
                for (int i = 0; i < maxId; i++)
                    IdGenerator.NuevoIdProducto();
            }
        }

        public List<Producto> GetAll() => _jsonRepo.GetAll();

        public Producto? GetById(int id)
        {
            return _jsonRepo.GetById(p => p.Id == id);
        }

        public Producto Add(Producto producto)
        {
            producto.Id = IdGenerator.NuevoIdProducto();
            _jsonRepo.Add(producto);
            return producto;
        }

        public void Update(Producto producto)
        {
            _jsonRepo.UpdateItem(producto, p => p.Id == producto.Id);
        }

        public void Delete(int id)
        {
            _jsonRepo.Delete(p => p.Id == id);
        }
    }
}

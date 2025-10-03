using AmperImp.Models;
using AmperImp.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AmperImp.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ProductoRepository _repo;

        public ProductosController(IWebHostEnvironment env)
        {
            var path = Path.Combine(env.ContentRootPath, "Data", "productos.json");
            _repo = new ProductoRepository(path);
        }

        // Index solo muestra el filtro
        public IActionResult Index()
        {
            ViewBag.Categorias = Enum.GetValues(typeof(CategoriaProducto)).Cast<CategoriaProducto>().ToList();
            ViewBag.Talles = Enum.GetValues(typeof(Talle)).Cast<Talle>().ToList();
            return View();
        }

        // Partial para mostrar resultados filtrados
        public IActionResult ViewAll(string nombre = "", CategoriaProducto? categoria = null, Talle? talle = null)
        {
            var productos = _repo.GetAll();

            if (!string.IsNullOrEmpty(nombre))
                productos = productos.Where(p => p.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase)).ToList();

            if (categoria != null)
                productos = productos.Where(p => p.Categoria == categoria).ToList();

            if (talle != null)
                productos = productos.Where(p => p.StockTalles.Any(s => s.Talle == talle && s.Cantidad > 0)).ToList();

            return PartialView("_ViewAll", productos);
        }

        // CreateOrEdit popup
        public IActionResult CreateOrEdit(int? id)
        {
            Producto model = id == null ? new Producto() : _repo.GetById(id.Value)!;
            ViewBag.Categorias = Enum.GetValues(typeof(CategoriaProducto)).Cast<CategoriaProducto>().ToList();
            ViewBag.Talles = Enum.GetValues(typeof(Talle)).Cast<Talle>().ToList();
            return PartialView("_CreateOrEdit", model);
        }

        [HttpPost]
        public IActionResult CreateOrEdit(Producto model, [FromForm] Dictionary<Talle, int> StockTalles, IFormFile? ImagenFile)
        {
            model.StockTalles = StockTalles.Select(kv => new StockTalle
            {
                Talle = kv.Key,
                Cantidad = kv.Value
            }).ToList();

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = Enum.GetValues(typeof(CategoriaProducto)).Cast<CategoriaProducto>().ToList();
                ViewBag.Talles = Enum.GetValues(typeof(Talle)).Cast<Talle>().ToList();
                return PartialView("_CreateOrEdit", model);
            }

            if (model.Id == 0)
                _repo.Add(model);
            else
                _repo.Update(model);

            return Json(new { success = true });
        }

        // Show popup
        public IActionResult Show(int id)
        {
            var model = _repo.GetById(id);

            if (model == null)
            {
                return NotFound();
            }

            // Retornar la partial view para cargarla en el modal
            return PartialView("_Show", model);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            _repo.Delete(id);
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult GetStockPorTalle(int id)
        {
            var producto = _repo.GetById(id);
            if (producto == null) return NotFound();

            // devolvemos { "S": 5, "M": 2, "L": 0, ... }
            var stock = producto.StockTalles.ToDictionary(st => st.Talle.ToString(), st => st.Cantidad);
            return Json(stock);
        }
    }
}

using AmperImp.Models;
using AmperImp.Repositories; // Si usás repositorio tipo productos
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AmperImp.Controllers
{
    public class VentasController : Controller
    {
        private readonly VentaRepository _ventasRepository;
        private readonly ProductoRepository _productoRepository;

        public VentasController(VentaRepository ventasRepository, ProductoRepository productoRepository)
        {
            _ventasRepository = ventasRepository;
            _productoRepository = productoRepository;
        }

        // GET: Ventas
        public IActionResult Index()
        {
            ViewBag.Categorias = Enum.GetValues(typeof(CategoriaProducto)).Cast<CategoriaProducto>().ToList();
            ViewBag.Talles = Enum.GetValues(typeof(Talle)).Cast<Talle>().ToList();
            ViewBag.Productos = _productoRepository.GetAll();
            return View();
        }

        //Partial para mostrar resultados filtrados
        public IActionResult ViewAll(string comprador = "", string producto = "", DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var ventas = _ventasRepository.GetAll();

            if (!string.IsNullOrEmpty(comprador))
                ventas = ventas
                    .Where(v => v.Detalles.Any(d => d.Comprador.Contains(comprador, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

            if (!string.IsNullOrEmpty(producto))
                ventas = ventas
                    .Where(v => v.Detalles.Any(d => d.NombreProducto.Contains(producto, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

            if (fechaDesde != null)
                ventas = ventas.Where(v => v.Fecha >= fechaDesde).ToList();

            if (fechaHasta != null)
                ventas = ventas.Where(v => v.Fecha <= fechaHasta).ToList();

            return PartialView("_ViewAll", ventas);
        }

        public IActionResult Show(int id)
        {
            var model = _ventasRepository.GetById(id);
            return PartialView("_Show", model);
        }

        // GET
        [HttpGet]
        public IActionResult CreateOrEdit(int? id)
        {
            ViewBag.Talles = Enum.GetValues(typeof(Talle)).Cast<Talle>().ToList();
            ViewBag.Productos = _productoRepository.GetAll();

            Venta model = id == null ? new Venta() : _ventasRepository.GetById(id.Value) ?? new Venta();
            return PartialView("_CreateOrEdit", model);
        }

        // POST
        [HttpPost]
        public IActionResult CreateOrEdit([FromBody] Venta model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = Enum.GetValues(typeof(CategoriaProducto)).Cast<CategoriaProducto>().ToList();
                ViewBag.Talles = Enum.GetValues(typeof(Talle)).Cast<Talle>().ToList();
                return PartialView("_CreateOrEdit", model);
            }

            var existing = _ventasRepository.GetById(model.Id);
            if (existing != null)
            {
                existing.Detalles = model.Detalles;
                existing.Fecha = model.Fecha;
                existing.Comprador = model.Comprador;
                _ventasRepository.Update(existing);
            }
            else
            {
                _ventasRepository.Add(model);
            }

            foreach (var d in model.Detalles)
            {
                var producto = _productoRepository.GetById(d.ProductoId);
                if (producto != null)
                {
                    var stockTalle = producto.StockTalles.FirstOrDefault(s => s.Talle == d.Talle);
                    if (stockTalle != null)
                    {
                        if (stockTalle.Cantidad >= d.Cantidad)
                            stockTalle.Cantidad -= d.Cantidad;
                    }

                    _productoRepository.Update(producto); // guardar el cambio de stock
                }
            }

            return Json(new { success = true });
        }

        // AgregarDetalle
        [HttpPost]
        public IActionResult AgregarDetalle(int? ventaId, int productoId, Talle talle, int cantidad)
        {
            // Obtenemos o creamos venta temporal
            Venta venta;
            if (ventaId == null || ventaId == 0)
                venta = new Venta();
            else
                venta = _ventasRepository.GetById(ventaId.Value) ?? new Venta { Id = ventaId.Value };

            var producto = _productoRepository.GetById(productoId);
            if (producto == null)
                return BadRequest("Producto no encontrado.");

            var detalle = new DetalleVenta
            {
                ProductoId = producto.Id,
                NombreProducto = producto.Nombre,
                Talle = talle,
                Cantidad = cantidad,
                PrecioUnitario = producto.PrecioVenta
            };

            venta.Detalles.Add(detalle);

            // Si es una venta existente, actualizamos el repo
            if (ventaId != null && ventaId != 0)
                _ventasRepository.Update(venta);

            // Devolvemos tabla parcial actualizada
            return PartialView("_DetallesTabla", venta.Detalles);
        }

        [HttpPost]
        public IActionResult EliminarDetalle(int ventaId, int productoId, Talle talle)
        {
            // Obtenemos la venta
            var venta = _ventasRepository.GetById(ventaId);
            if (venta == null)
                return BadRequest("Venta no encontrada.");

            // Buscamos el detalle correspondiente
            var detalle = venta.Detalles.FirstOrDefault(d => d.ProductoId == productoId && d.Talle == talle);
            if (detalle != null)
            {
                venta.Detalles.Remove(detalle);
                _ventasRepository.Update(venta);
            }

            // Devolvemos el partial actualizado
            return PartialView("_DetallesTabla", venta.Detalles);
        }
        
        //[HttpPost]
        //public IActionResult Delete(int id)
        //{
        //    _repo.Delete(id);
        //    return Json(new { success = true });
        //}
    }
}

// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

console.log("site.js cargado, jQuery:", typeof jQuery !== "undefined");

(function ($) {
    // guardamos bandera para evitar doble-binding
    if (!window._ventaHandlersInitialized) {

        // handler delegado para agregar detalle (funciona aunque el DOM se cargue dinámicamente)
        $(document).on("click", "#btnAgregarDetalle", function (e) {
            e.preventDefault();
            console.log("btnAgregarDetalle clicked");

            var productoId = $("#selectProducto").val();
            var productoNombre = $("#selectProducto option:selected").text();
            var precioUnitario = parseFloat($("#selectProducto option:selected").data("precio")) || 0;
            var talle = $("#selectTalle").val();
            var cantidad = parseInt($("#inputCantidad").val()) || 0;

            if (!productoId || !talle || cantidad <= 0) {
                alert("Seleccione producto, talle y cantidad válida.");
                return;
            }

            // si existe fila vacía la removemos
            $("#filaVacia").remove();

            var subtotal = (precioUnitario * cantidad).toFixed(2);
            var fila = `<tr data-productoid="${productoId}" data-talle="${talle}" data-precio="${precioUnitario}">
                <td class="text-start">${productoNombre}</td>
                <td>${talle}</td>
                <td class="td-cantidad">${cantidad}</td>
                <td class="td-precio">${precioUnitario.toLocaleString('es-AR', { style: 'currency', currency: 'ARS' })}</td>
                <td class="td-sub">${(precioUnitario * cantidad).toLocaleString('es-AR', { style: 'currency', currency: 'ARS' })}</td>
                <td>
                  <button type="button" class="btn btn-sm btn-danger btnEliminarDetalle">
                    <i class="fa fa-trash"></i>
                  </button>
                </td>
              </tr>`;

            // Si la tabla ya existe, append; si no, crear (pero con tabla presente en DOM preferible)
            if ($("#tablaDetalles").length === 0) {
                var tablaHtml = `<table class="table table-sm table-bordered table-striped align-middle text-center" id="tablaDetalles">
            <thead>
              <tr>
                <th>Producto</th><th>Talle</th><th>Cantidad</th><th>Precio Unitario</th><th>Subtotal</th><th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              ${fila}
            </tbody>
          </table>`;
                $("#divDetalles").html(tablaHtml);
            } else {
                $("#tablaDetalles tbody").append(fila);
            }

            // limpiar inputs
            $("#selectProducto").val("");
            $("#selectTalle").val("");
            $("#inputCantidad").val(1);
        });

        // handler delegado para eliminar fila
        $(document).on("click", ".btnEliminarDetalle", function (e) {
            e.preventDefault();
            $(this).closest("tr").remove();
            // si ya no quedan filas, dejamos la fila vacía
            if (tbody.find("tr").length === 0) {
                tbody.append('<tr id="filaVacia"><td colspan="6" class="text-center text-muted">No hay detalles cargados.</td></tr>');
            }
        });

        window._ventaHandlersInitialized = true;
        console.log("handlers de venta inicializados");

        $(document).on("submit", "#formVenta", function (e) {
            e.preventDefault();

            var detalles = [];
            var comprador = $("input[name='Comprador']").val();

            $("#tablaDetalles tbody tr").each(function () {
                if ($(this).attr("id") === "filaVacia") return;

                detalles.push({
                    ProductoId: $(this).data("productoid"),
                    Comprador: comprador,
                    NombreProducto: $(this).find("td:eq(0)").text(),
                    Talle: $(this).data("talle").toString(),
                    Cantidad: parseInt($(this).find("td:eq(2)").text()),
                    PrecioUnitario: parseFloat($(this).data("precio"))
                });
            });

            var model = {
                Id: parseInt($("input[name='Id']").val()) || 0,
                Fecha: $("input[name='Fecha']").val(),
                Comprador: comprador,
                Detalles: detalles
            };

            $.ajax({
                url: $(this).attr("action"),
                type: "POST",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(model),
                success: function (res) {
                    if (res.success) {
                        $("#modalGenerico").modal("hide");
                        cargarTabla();
                    }
                },
                error: function (xhr) {
                    alert("Error al guardar la venta: " + xhr.responseText);
                }
            });
        });
    }

})(jQuery);
using AmperImp.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var pathVentas = Path.Combine(builder.Environment.ContentRootPath, "Data", "ventas.json");
var pathProductos = Path.Combine(builder.Environment.ContentRootPath, "Data", "productos.json");

builder.Services.AddSingleton<VentaRepository>(sp => new VentaRepository(pathVentas));
builder.Services.AddSingleton<ProductoRepository>(sp => new ProductoRepository(pathProductos));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using System.Globalization;
using Eventos.Web.Data;
using Eventos.Web.Repositorios;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    var mensajes = options.ModelBindingMessageProvider;
    mensajes.SetValueMustNotBeNullAccessor(_ => "Este campo es obligatorio.");
    mensajes.SetMissingBindRequiredValueAccessor(_ => "Este campo es obligatorio.");
    mensajes.SetMissingKeyOrValueAccessor(() => "Este campo es obligatorio.");
    mensajes.SetAttemptedValueIsInvalidAccessor((valor, _) => $"El valor '{valor}' no es válido.");
    mensajes.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"El valor '{valor}' no es válido.");
    mensajes.SetValueIsInvalidAccessor(valor => $"El valor '{valor}' no es válido.");
    mensajes.SetUnknownValueIsInvalidAccessor(_ => "El valor ingresado no es válido.");
    mensajes.SetNonPropertyUnknownValueIsInvalidAccessor(() => "El valor ingresado no es válido.");
    mensajes.SetValueMustBeANumberAccessor(_ => "Debe ingresar un número.");
    mensajes.SetNonPropertyValueMustBeANumberAccessor(() => "Debe ingresar un número.");
})
.AddViewOptions(options => options.HtmlHelperOptions.ClientValidationEnabled = false);

builder.Services.AddDbContext<EventosContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Eventos")));

builder.Services.AddScoped<EventoRepositorio>();
builder.Services.AddScoped<AsistenteRepositorio>();
builder.Services.AddScoped<InscripcionRepositorio>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventosContext>();
    if ((await context.Database.GetPendingMigrationsAsync()).Any())
        app.Logger.LogWarning("Hay migraciones pendientes. Ejecute Update-Database antes de iniciar la aplicación.");
    else
        await DatosIniciales.CargarAsync(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStatusCodePagesWithReExecute("/Home/Estado/{0}");

var cultura = new CultureInfo("es-PE");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = [cultura],
    SupportedUICultures = [cultura]
});

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Eventos}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

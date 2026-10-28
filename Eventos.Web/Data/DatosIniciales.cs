using Eventos.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Web.Data;

public static class DatosIniciales
{
    public static async Task CargarAsync(EventosContext context)
    {
        if (!await context.Categorias.AnyAsync())
        {
            context.Categorias.AddRange(
                new Categoria { Nombre = "Tecnología" },
                new Categoria { Nombre = "Negocios" },
                new Categoria { Nombre = "Cultura" });
            await context.SaveChangesAsync();
        }

        var categorias = await context.Categorias.ToDictionaryAsync(c => c.Nombre, c => c.Id);
        if (!await context.Eventos.AnyAsync() &&
            categorias.TryGetValue("Tecnología", out var tecnologia) &&
            categorias.TryGetValue("Negocios", out var negocios) &&
            categorias.TryGetValue("Cultura", out var cultura))
        {
            var hoy = DateTime.Today;

            context.Eventos.AddRange(
                new Evento
                {
                    Titulo = "Encuentro de desarrolladores de software",
                    Fecha = hoy.AddDays(14).AddHours(18).AddMinutes(30),
                    Lugar = "Auditorio principal, San Isidro",
                    Aforo = 120,
                    Precio = 0m,
                    CategoriaId = tecnologia
                },
                new Evento
                {
                    Titulo = "Seguridad en aplicaciones web",
                    Fecha = hoy.AddDays(-6).AddHours(19),
                    Lugar = "Sala 2, Centro Empresarial Real",
                    Aforo = 60,
                    Precio = 35m,
                    CategoriaId = tecnologia
                },
                new Evento
                {
                    Titulo = "Mentoría grupal para emprendedores",
                    Fecha = hoy.AddDays(21).AddHours(10),
                    Lugar = "Oficina 501, Miraflores",
                    Aforo = 4,
                    Precio = 50m,
                    CategoriaId = negocios
                },
                new Evento
                {
                    Titulo = "Noche de cine peruano",
                    Fecha = hoy.AddDays(30).AddHours(20),
                    Lugar = "Centro cultural, Barranco",
                    Aforo = 150,
                    Precio = 15m,
                    CategoriaId = cultura
                });
            await context.SaveChangesAsync();
        }

        if (!await context.Asistentes.AnyAsync())
        {
            context.Asistentes.AddRange(
                new Asistente { Nombres = "Lucía Paredes Quispe", Dni = "45879123", Correo = "lucia.paredes@gmail.com" },
                new Asistente { Nombres = "Carlos Mendoza Rojas", Dni = "70214589", Correo = "cmendoza.rojas@outlook.com" },
                new Asistente { Nombres = "Andrea Salazar Huamán", Dni = "72563014", Correo = "andrea.salazar@gmail.com" },
                new Asistente { Nombres = "Diego Castillo Vargas", Dni = "46931275", Correo = "diego.castillo@hotmail.com" },
                new Asistente { Nombres = "Valeria Ríos Torres", Dni = "71840236", Correo = "valeria.rios@gmail.com" });
            await context.SaveChangesAsync();
        }
    }
}

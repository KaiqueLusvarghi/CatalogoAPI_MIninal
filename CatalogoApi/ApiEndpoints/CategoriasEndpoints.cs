using CatalogoApi.Context;
using CatalogoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.ApiEndpoints;

public static class CategoriasEndpoints
{
    public static void MapCategoriasEndpoints(this WebApplication app)
    {
        app.MapPost("/categorias", async (Categoria categoria, AppDbContex db) =>
        {
            db.Categorias.Add(categoria);
            await db.SaveChangesAsync();

            return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);
        });

        app.MapGet("/categorias", async (AppDbContex db) =>
                await db.Categorias.ToListAsync()).WithTags("Categorias").RequireAuthorization(); ;//protegendo endpoint

        app.MapGet("/categorias/{id:int}", async (int id, AppDbContex db) =>
        {
            return await db.Categorias.FindAsync(id)
            is Categoria categoria
            ? Results.Ok(categoria)
            : Results.NotFound($"Categoria {id}  não foi encontrada");
        });

        app.MapPut("/categorias/{id:int}", async (int id, Categoria categoria, AppDbContex db) =>
        {
            if (categoria.CategoriaId != id)
            {
                return Results.BadRequest($"Não foi possível fazer a alteração, tente novamente");
            }
            var categoriaDB = await db.Categorias.FindAsync(id);
            if (categoriaDB is null) return Results.NotFound($"Categoria com o id {id} não Existe !");

            categoriaDB.Nome = categoria.Nome;
            categoriaDB.Descricao = categoria.Descricao;

            await db.SaveChangesAsync();
            return Results.Ok(categoriaDB);

        });

        app.MapDelete("/categorias/{id:int}", async (int id, AppDbContex db) =>
        {
            var categoria = await db.Categorias.FindAsync(id);
            if (categoria is null)
            {
                return Results.NotFound($"Não foi possível excluir essa categoria, pois a categoria {id} não foi encontra !");
            }

            db.Categorias.Remove(categoria);
            await db.SaveChangesAsync();

            return Results.Content($"Categoria {id} escluida com sucesso");
        });
    }
}

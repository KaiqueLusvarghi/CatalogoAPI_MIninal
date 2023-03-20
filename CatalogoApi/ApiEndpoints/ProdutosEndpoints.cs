using CatalogoApi.Context;
using CatalogoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.ApiEndpoints;

public static class ProdutosEndpoints
{
    public static void MapProdutosEndpoints(this WebApplication app)
    {
        app.MapPost("/produtos", async (Produto produto, AppDbContex db) =>
        {
            db.Produtos.Add(produto);
            await db.SaveChangesAsync();

            return Results.Created($"/produtos/{produto.ProdutoId}", produto);

        });

        app.MapGet("/produtos", async (AppDbContex db) => await db.Produtos.ToListAsync()).WithTags("Produtos").RequireAuthorization(); //protegendo endpoint

        app.MapGet("/produtos/{id:int}", async (int id, AppDbContex db) =>
        {
            return await db.Produtos.FindAsync(id)
                    is Produto produto
                    ? Results.Ok(produto)
                    : Results.NotFound($"Produto com o {id} não foi encontrado");
        });

        app.MapPut("/produtos/{id:int}", async (int id, Produto produto, AppDbContex db) =>
        {
            if (produto.ProdutoId != id)
            {
                return Results.BadRequest($"Não foi possível fazer a alteração, tente novamente");
            }
            var produtoDB = await db.Produtos.FindAsync(id);
            if (produtoDB is null) return Results.NotFound($"Produto com o id {id} não Existe !");

            produtoDB.Nome = produto.Nome;
            produtoDB.Descricao = produto.Descricao;

            await db.SaveChangesAsync();
            return Results.Ok(produtoDB);

        });

        app.MapDelete("/produtos/{id:int}", async (int id, AppDbContex db) =>
        {
            var produto = await db.Produtos.FindAsync(id);
            if (produto is null)
            {
                return Results.NotFound($"Não foi possível excluir essa categoria, pois a categoria {id} não foi encontra !");
            }

            db.Produtos.Remove(produto);
            await db.SaveChangesAsync();

            return Results.Content($"produto  {id} escluido com sucesso");
        });
    }
}

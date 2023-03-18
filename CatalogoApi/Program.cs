using CatalogoApi.Context;
using CatalogoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContex>(options =>
                    options
                    .UseMySql(connectionString,
                    ServerVersion.AutoDetect(connectionString)));


var app = builder.Build();

//definindfo os endpoints 

//Categorias 

app.MapPost("/categorias", async (Categoria categoria, AppDbContex db) =>
{
    db.Categorias.Add(categoria);
    await db.SaveChangesAsync();

    return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);
});

app.MapGet("/categorias", async(AppDbContex db) => 
        await db.Categorias.ToListAsync());

app.MapGet("/categorias/{id:int}", async (int id, AppDbContex db) =>
{
    return await db.Categorias.FindAsync(id)
    is Categoria categoria
    ? Results.Ok(categoria)
    :Results.NotFound($"Categoria {id}  não foi encontrada");
});

app.MapPut("/categorias/{id:int}", async (int id, Categoria categoria, AppDbContex db) =>
{
    if(categoria.CategoriaId != id)
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

//Produtos

app.MapPost("/produtos", async (Produto produto, AppDbContex db) =>
{
    db.Produtos.Add(produto);
    await db.SaveChangesAsync();

    return Results.Created($"/produtos/{produto.ProdutoId}", produto);

});

app.MapGet("/produtos", async (AppDbContex db) =>await db.Produtos.ToListAsync());

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


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}






app.Run();


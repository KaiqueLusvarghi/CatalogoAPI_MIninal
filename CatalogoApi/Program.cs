using CatalogoApi.Context;
using CatalogoApi.Models;
using CatalogoApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>      //configurando o swagger para o JWT
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiCatalogo", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header using the Bearer scheme.
                    Enter 'Bearer'[space].Example: \'Bearer 12345abcdef\'",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                          {
                              Reference = new OpenApiReference
                              {
                                  Type = ReferenceType.SecurityScheme,
                                  Id = "Bearer"
                              }
                          },
                         new string[] {}
                    }
                });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContex>(options =>
                    options
                    .UseMySql(connectionString,
                    ServerVersion.AutoDetect(connectionString)));

builder.Services.AddSingleton<ITokenService>(new TokenService()); //Registrando o serviço

builder.Services.AddAuthentication //validação do token
                 (JwtBearerDefaults.AuthenticationScheme)
                 .AddJwtBearer(options =>
                 {
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         ValidateIssuerSigningKey = true,

                         ValidIssuer = builder.Configuration["Jwt:Issuer"],
                         ValidAudience = builder.Configuration["Jwt:Audience"],
                         IssuerSigningKey = new SymmetricSecurityKey
                         (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                     };
                 });

builder.Services.AddAuthorization(); // serviço de autorização 


var app = builder.Build();

//endpoint para o login

app.MapPost("/login", [AllowAnonymous] (UserModel userModel, ITokenService tokenService) =>
{
    if (userModel == null)
    {
        return Results.BadRequest("Login Inválido");
    }
    if (userModel.UserName == "kaique" && userModel.Password == "12345")
    {
        var tokenString = tokenService.GerarToken(app.Configuration["Jwt:Key"],
            app.Configuration["Jwt:Issuer"],
            app.Configuration["Jwt:Audience"],
            userModel);
        return Results.Ok(new { token = tokenString });
    }
    else
    {
        return Results.BadRequest("Login Inválido");
    }
}).Produces(StatusCodes.Status400BadRequest)
              .Produces(StatusCodes.Status200OK)
              .WithName("Login")
              .WithTags("Autenticacao");


//definindfo os endpoints 

//Categorias 

app.MapPost("/categorias", async (Categoria categoria, AppDbContex db) =>
{
    db.Categorias.Add(categoria);
    await db.SaveChangesAsync();

    return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);
});

app.MapGet("/categorias", async(AppDbContex db) => 
        await db.Categorias.ToListAsync()).WithTags("Categorias").RequireAuthorization(); ;//protegendo endpoint

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

app.MapGet("/produtos", async (AppDbContex db) =>await db.Produtos.ToListAsync()).WithTags("Produtos").RequireAuthorization(); //protegendo endpoint

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

//ativando os serviços de autorização e autenticação 
app.UseAuthentication();
app.UseAuthorization();



app.Run();


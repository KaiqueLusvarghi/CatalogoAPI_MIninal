using CatalogoApi.Models;
using Microsoft.EntityFrameworkCore;


namespace CatalogoApi.Context
{
    public class AppDbContex : DbContext
    {
        public AppDbContex(DbContextOptions<AppDbContex> options) : base(options)
        {

        }

        // Dbset serve para fazer as consultas do BD
       public DbSet<Produto>? Produtos { get; set; }

       public DbSet<Categoria>? Categorias { get; set; }


       protected override void OnModelCreating(ModelBuilder mb)
       {
            //Categoria

            //definindo categoriaID como PK
            mb.Entity<Categoria>().HasKey(c => c.CategoriaId); 
            //colocando o tamanho maximo do nome e que é obrigado apreencher(required)
            mb.Entity<Categoria>().Property(c =>c.Nome)
                                  .HasMaxLength(100)
                                  .IsRequired();
            //Property esta definindo uma propriedade (Descrição) 
            mb.Entity<Categoria>().Property(c => c.Descricao) .HasMaxLength(150).IsRequired();


            //Produto

            mb.Entity<Produto>().HasKey(c => c.ProdutoId);
            mb.Entity<Produto>().Property(c => c.Nome).HasMaxLength(100).IsRequired();
            mb.Entity<Produto>().Property(c => c.Descricao).HasMaxLength(150).IsRequired();
            mb.Entity<Produto>().Property(c => c.Imagem).HasMaxLength(100);
            mb.Entity<Produto>().Property(c=> c.Preco).HasPrecision(14,2);



            //Relacionamento

            mb.Entity<Produto>()
                .HasOne<Categoria>(c=> c.Categoria) //HasOne configura o lado 1 do relacionamento
                .WithMany(p=> p.Produtos) // permite indicar que a propriedade contem o relacionamento do tipo muitos 
                .HasForeignKey(c => c.CategoriaId); //definindo a chave estrangeirta FK

        }
    }
}

using CatalogoApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CatalogoApi.Services
{
    public class TokenService : ITokenService
    {
        public string GerarToken(string key, string issuer, string audience, UserModel user)
        {
            var claims = new[]
            {
              new Claim(ClaimTypes.Name,user.UserName),
              new Claim(ClaimTypes.NameIdentifier,Guid.NewGuid().ToString())
            };

   //Pegando a chave secreta e codificando e pegando a classe SymmetricSecurityKey para gerar uma chave segura 
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

    //aplicando um algoritmo nessa chave (HmacSha256) e ai eu tenho minha chave simetrica
            var credentials = new SigningCredentials(securitykey,SecurityAlgorithms.HmacSha256);
    
            //definindo a geração do token 
            var token = new JwtSecurityToken(issuer : issuer,
                                             audience : audience, 
                                             claims: claims,
                                             expires: DateTime.Now.AddMinutes(120),
                                             signingCredentials: credentials);

    // desearializar o token e retornar uma estring para o usuario e vai representar o token Jwt
             var tokenHandler = new JwtSecurityTokenHandler();
             var stringToken = tokenHandler.WriteToken(token);
             return stringToken;
        }
    }
}

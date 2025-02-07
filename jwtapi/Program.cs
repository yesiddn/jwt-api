using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

string key = "jb11jNVBBNUKYsDKitkmK3qUzf4CAr3asVp5pk0D"; // debe ser mayor a 256 bits

builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer")
  .AddJwtBearer(opt =>
  {
    var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));

    var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature);

    // las propiedades que se marcan como false, son para que no valide el token con el servidor de identidad ya que estamos generando el token nosotros mismos
    opt.RequireHttpsMetadata = false;

    opt.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = false, // valida el emisor del token
      ValidateAudience = false, // valida el receptor del token
      IssuerSigningKey = signingKey,
    };
  });

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();

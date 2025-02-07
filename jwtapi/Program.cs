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
app.MapGet("/protected", (ClaimsPrincipal user) => "Route protected! User: " + user.Identity?.Name)
  .RequireAuthorization();

app.MapGet("/protectedwithscope", (ClaimsPrincipal user) => $"Route protected with scope! User: {user.Identity?.Name} - Scope: {user.FindFirst("scope")?.Value}")
  .RequireAuthorization(p => p.RequireClaim("scope", "myapi:drunken")); // el scope es como tener roles en la aplicación

app.MapGet("/login/{user}/{password}", (string user, string password) => {
  if (user == "pato" &&  password == "donal")
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var byteKey = Encoding.ASCII.GetBytes(key);
    var tokenDes = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(new Claim  []
      {
        new Claim(ClaimTypes.Name, user), // el claim recibe un string con el nombre del claim (puede ser cualquier cosa) y el valor
        new Claim("scope", "myapi:drunken")
      }), // los claims son los datos que se van a guardar en el token
      Expires = DateTime.UtcNow.AddHours(1), // tiempo de expiración del token -> AddDays, AddMinutes, AddMonths, etc.
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(byteKey), SecurityAlgorithms.HmacSha256Signature) // la misma que usamos en AddJwtBearer
    };

    var token = tokenHandler.CreateToken(tokenDes);

    return Results.Ok(tokenHandler.WriteToken(token)); // retorna el token generado como un string
  } 
  else
  {
    return Results.Unauthorized();
  }
});

app.Run();

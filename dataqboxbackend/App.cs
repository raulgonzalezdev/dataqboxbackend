using dataqboxbackend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using StackExchange.Redis;
using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Configuración de claves y valores desde el archivo de configuración
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("La clave 'Jwt:Key' no está configurada.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("La clave 'Jwt:Issuer' no está configurada.");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("La clave 'Jwt:Audience' no está configurada.");
var googleClientId = builder.Configuration["Google:ClientId"] ?? throw new InvalidOperationException("La clave 'Google:ClientId' no está configurada.");
var googleClientSecret = builder.Configuration["Google:ClientSecret"] ?? throw new InvalidOperationException("La clave 'Google:ClientSecret' no está configurada.");

// Configuración de autenticación
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie()
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
})
.AddGoogle(options =>
{
    options.ClientId = googleClientId;
    options.ClientSecret = googleClientSecret;
    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.SaveTokens = true; // Guarda los tokens para su uso posterior
});

// Configuración de Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost")); // Cambia "localhost" por la dirección de tu servidor Redis si es necesario

// Configuración de la base de datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Agregar controladores y servicios adicionales
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger con un filtro para ocultar endpoints
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Agregar filtro para ocultar endpoints marcados con un atributo personalizado
    c.DocumentFilter<HideInProductionFilter>();
});

// Configuración de sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración de la sesión
    options.Cookie.HttpOnly = true; // Asegúrate de que la cookie sea solo HTTP
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSession();

// Configurar Swagger para mostrar u ocultar según el entorno
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Clase para ocultar endpoints en producción
public class HideInProductionFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Eliminar las rutas marcadas con el atributo `[HideInProduction]`
        var pathsToRemove = swaggerDoc.Paths
            .Where(p => p.Value.Operations.Values.Any(op =>
                op.Extensions.ContainsKey("x-hide-in-production") &&
                bool.Parse(op.Extensions["x-hide-in-production"].ToString() ?? "false")))
            .Select(p => p.Key)
            .ToList();

        foreach (var path in pathsToRemove)
        {
            swaggerDoc.Paths.Remove(path);
        }
    }
}

// Atributo personalizado para marcar los endpoints
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class HideInProductionAttribute : Attribute
{
    public bool Hide { get; set; } = true;
}

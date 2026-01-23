using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Servicios;
using API_SSO.Servicios.Contratos;
using API_SSO.Utilidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//Obtiene la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("ssoConection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

//Configura el DbContext
builder.Services.AddDbContext<SSOContext>(options =>
    options.UseSqlServer(connectionString));

//Configura el identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Ajusta según tu necesidad
    options.Password.RequireDigit = false;          // Ajusta para pruebas
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<SSOContext>()
.AddDefaultTokenProviders();

//Configura los CORS y otros servicios
var origenesPermitidos = builder.Configuration.GetValue<string>("OrigenesPermitidos")!.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
if (origenesPermitidos.Length == 0)
    throw new InvalidOperationException("Falta 'OrigenesPermitidos' en configuración.");

builder.Services.AddCors(zOptions =>
{
    //zOptions.AddDefaultPolicy(zBuilder =>
    //{
    //    zBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
    //      .WithExposedHeaders(new string[] { "CantidadTotalRegistros" });
    //});

    var allowedHosts = builder.Configuration.GetSection("AllowedHosts").Get<string[]>();

    if (allowedHosts != null && allowedHosts.Length > 0)
    {
        zOptions.AddDefaultPolicy(builder =>
        {
            builder.WithOrigins(allowedHosts)
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    }
    else
    {
        // Configuración de CORS por defecto si no hay AllowedHosts configurados
        zOptions.AddDefaultPolicy(builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    }


});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.InyectarDependencias(builder.Configuration);

builder.Services.Configure<GraphOptions>(
    builder.Configuration.GetSection("Graph"));

var cfg = builder.Configuration;

builder.Services.AddSingleton<IEmailService>(_ =>
    new EmailService(
        cfg["Graph:TenantId"]!,
        cfg["Graph:ClientId"]!,
        cfg["Graph:ClientSecret"]!

    )
);

var app = builder.Build();

// 5. Configuración del Pipeline (Middleware)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();

// El orden aquí es vital para la seguridad
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
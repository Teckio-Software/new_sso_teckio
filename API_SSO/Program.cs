using API_SSO.Context;
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
var origenesPermitidos = builder.Configuration.GetValue<string>("OrigenesPermitidos")!.Split(",");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.InyectarDependencias(builder.Configuration);

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

// El orden aquí es vital para la seguridad
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
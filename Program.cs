using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
using AuthServiceNamespace.Services;
using AuthServiceNamespace.Middleware;
using AuthService.Config;
using CarreraService.Services;


Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registro del servicio IAuthService
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthServiceNamespace.Services.AuthService>();

// Configuración de la base de datos
var connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING") + ";TrustServerCertificate=True;";
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(connectionString));


// Configuración de JWT
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
var key = Encoding.ASCII.GetBytes(jwtSecret);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.Configure<RabbitMQSettings>(options =>
{
    options.Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
    options.Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672");
    options.User = Environment.GetEnvironmentVariable("RABBITMQ_USER");
    options.Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
    options.Queue = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE");
    options.Exchange = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE");
    options.RoutingKey = Environment.GetEnvironmentVariable("RABBITMQ_ROUTING_KEY");
});

builder.Services.AddSingleton<RabbitMQConnection>();
builder.Services.AddSingleton< RabbitMQPublisher>();
builder.Services.AddHostedService<RabbitMQConsumer>();


var app = builder.Build();

// Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseMiddleware<JwtValidationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

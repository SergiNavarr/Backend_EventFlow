using Datos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Negocio.Services;
using Negocio.Interfaces;
using Negocio.Hubs;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") // La URL exacta de tu frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Importante si usas cookies o headers de auth
        });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var connectionString = builder.Configuration.GetConnectionString("CadenaConexionPostgres");

builder.Services.AddDbContext<EventflowDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

var key = builder.Configuration["Jwt:Key"];

builder.Services.AddAuthentication(config => {
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config => {
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false, // Por ahora false para facilitar pruebas locales
        ValidateAudience = false // Por ahora false para facilitar pruebas locales
    };
});

//Configuracion de SignalR
builder.Services.AddSignalR();

//Agregacion de servicios propios
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();
builder.Services.AddScoped<IPostService, PostService>();

builder.Services.AddScoped<IEventService, EventService>();

builder.Services.AddScoped<IChatService, ChatService>();

// Estrategia dual de emails
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailService, EmailServiceDev>();
    Console.WriteLine("[EMAIL] Usando MailKit (Dev)");
}
else
{
    builder.Services.AddScoped<IEmailService, EmailServiceProd>();
    Console.WriteLine("[EMAIL] Usando SendGrid (Prod)");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors("AllowNextApp");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chathub");

app.Run();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chathub");

app.Run();
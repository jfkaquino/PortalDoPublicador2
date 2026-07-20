using Microsoft.EntityFrameworkCore;
using PortalDoPublicador.Api.Features;
using PortalDoPublicador.Shared.Infrastructure.Data;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<PortalDoPublicador.Api.Infrastructure.Data.ApiDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

// Forward AppDbContext resolution to ApiDbContext so other injected services still work
builder.Services.AddScoped<SharedDbContext>(sp => sp.GetRequiredService<PortalDoPublicador.Api.Infrastructure.Data.ApiDbContext>());

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapSyncEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();

app.MapGet("/", () => "API do Portal do Publicador está online!")
.WithName("Root");

app.Run();

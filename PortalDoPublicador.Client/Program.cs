using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using PortalDoPublicador.Client;
using PortalDoPublicador.Shared.Features.Perfis;
using PortalDoPublicador.Shared.Infrastructure.Data;
using Blazor.IndexedDB;
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configurar o SQLite no WASM
builder.Services.AddDbContext<SharedDbContext>((sp, options) =>
{
    var jsRuntime = sp.GetRequiredService<IJSRuntime>();
    options.UseSqlite("Filename=app.db");
});

var dbStore = new DbStore
{
    DbName = "MeuAppOfflineDb",
    Version = 1
};
dbStore.Stores.Add(new StoreSchema { Name = "FilaComandos", PrimaryKey = new IndexSpec { Name = "id", KeyPath = "id", Auto = false } });
dbStore.Stores.Add(new StoreSchema { Name = "FilaPull", PrimaryKey = new IndexSpec { Name = "id", KeyPath = "id", Auto = true } });
dbStore.Stores.Add(new StoreSchema { Name = "Configuracoes", PrimaryKey = new IndexSpec { Name = "chave", KeyPath = "chave", Auto = false } });

builder.Services.AddSingleton(dbStore);
builder.Services.AddScoped<IIndexedDbFactory, IndexedDbFactory>();
builder.Services.AddScoped<IndexedDBManager>();
builder.Services.AddScoped<PortalDoPublicador.Client.Infrastructure.Sync.PullProcessorService>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));
builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();

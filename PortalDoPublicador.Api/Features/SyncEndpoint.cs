using Microsoft.AspNetCore.Mvc;
using PortalDoPublicador.Api.Infrastructure.Data;
using PortalDoPublicador.Shared.Infrastructure.Sync;

namespace PortalDoPublicador.Api.Features;

public static class SyncEndpoint
{
    public static void MapSyncEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/sync", async (
            [FromBody] SyncRequest requisicao,
            ApiDbContext context,
            IServiceProvider serviceProvider) =>
        {
        });
    }
}
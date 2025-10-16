using CoreArchV2.Services.Arvento;
using CoreArchV2.Services.Arvento.Dto;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace CoreArchV2.Services.SignalR
{
    public class ArventoMapHub : Hub
    {
        private static ConcurrentDictionary<string, bool> ConnectedClients = new ConcurrentDictionary<string, bool>();
        public override async Task OnConnectedAsync()
        {
            ConnectedClients.TryAdd(Context.ConnectionId, true);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            ConnectedClients.TryRemove(Context.ConnectionId, out _);
            await base.OnDisconnectedAsync(exception);
        }
        public static bool HasConnectedClients()
        {
            return ConnectedClients.Count > 0;
        }
    }

    public class ArventoMapService : BackgroundService
    {
        private readonly IHubContext<ArventoMapHub> _hubContext;
        private readonly IServiceProvider _serviceProvider;
        public ArventoMapService(IHubContext<ArventoMapHub> hubContext,
           IServiceProvider serviceProvider)
        {
            _hubContext = hubContext;
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!ArventoMapHub.HasConnectedClients())//Hub bağlandıysa istek atmaya başla
                {
                    await Task.Delay(3000, stoppingToken);
                    continue;
                }

                var vehicleList = new List<ECoordinateDto>();
                using (var scope = _serviceProvider.CreateScope())
                {
                    var arventoService = scope.ServiceProvider.GetRequiredService<IArventoService>();
                    vehicleList = await arventoService.GetAracSonKoordinatList();
                }

                await _hubContext.Clients.All.SendAsync("ArventoMapPosition", vehicleList);
                await Task.Delay(3000, stoppingToken);
            }
        }
    }
}

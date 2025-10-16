using CoreArchV2.Core.Entity.Track;
using CoreArchV2.Services.Arvento;
using CoreArchV2.Services.Arvento.Dto;
using CoreArchV2.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace CoreArchV2.Services.SignalR
{
    public class BasaranVehicleMapHub : Hub
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

    public class BasaranMapService : BackgroundService
    {
        private readonly IHubContext<BasaranVehicleMapHub> _hubContext;
        private readonly IServiceProvider _serviceProvider;
        public BasaranMapService(IHubContext<BasaranVehicleMapHub> hubContext,
           IServiceProvider serviceProvider)
        {
            _hubContext = hubContext;
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!BasaranVehicleMapHub.HasConnectedClients())//Hub bağlandıysa istek atmaya başla
                {
                    await Task.Delay(3000, stoppingToken);
                    continue;
                }

                var vehicleList = new List<VehicleTracking>();
                using (var scope = _serviceProvider.CreateScope())
                {
                    var trackingService = scope.ServiceProvider.GetRequiredService<IVehicleMapService>();
                    vehicleList = await trackingService.GetTrackingCoordinateList();
                }

                await _hubContext.Clients.All.SendAsync("BasaranMapPosition", vehicleList);
                await Task.Delay(3000, stoppingToken);
            }
        }
    }
}

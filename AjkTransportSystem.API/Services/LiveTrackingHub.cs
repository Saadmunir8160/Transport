using Microsoft.AspNetCore.SignalR;

namespace AjkTransportSystem.API.Services
{
    public class LiveTrackingHub : Hub
    {
        public async Task JoinTripGroup(string tripId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"trip_{tripId}");
        }

        public async Task LeaveTripGroup(string tripId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"trip_{tripId}");
        }

        public async Task SendLocationUpdate(string tripId, double lat, double lng, double speed, string currentStop)
        {
            await Clients.Group($"trip_{tripId}").SendAsync("ReceiveLocationUpdate", new
            {
                TripId = tripId,
                Latitude = lat,
                Longitude = lng,
                Speed = speed,
                CurrentStop = currentStop,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task SendStopReachedAlert(string tripId, string stopName, string etaNext)
        {
            await Clients.Group($"trip_{tripId}").SendAsync("ReceiveStopAlert", new
            {
                TripId = tripId,
                StopName = stopName,
                EtaNext = etaNext,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

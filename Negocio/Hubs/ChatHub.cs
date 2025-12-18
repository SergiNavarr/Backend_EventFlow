using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Negocio.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinEventGroup(string eventId)
        {
            // Agregamos la conexión actual (Socket) al grupo llamado "eventId"
            await Groups.AddToGroupAsync(Context.ConnectionId, eventId);
        }

        public async Task LeaveEventGroup(string eventId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, eventId);
        }
    }
}
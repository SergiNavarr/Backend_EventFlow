using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Negocio.Hubs
{
    public class ChatHub : Hub
    {
        // El frontend llamará a este método cuando el usuario entre a la pantalla de un evento
        public async Task JoinEventGroup(string eventId)
        {
            // Agregamos la conexión actual (Socket) al grupo llamado "eventId"
            await Groups.AddToGroupAsync(Context.ConnectionId, eventId);
        }

        // El frontend llamará a esto cuando el usuario salga de la pantalla
        public async Task LeaveEventGroup(string eventId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, eventId);
        }
    }
}
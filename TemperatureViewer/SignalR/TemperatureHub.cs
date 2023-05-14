using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TemperatureViewer.BackgroundNAccessServices;

namespace TemperatureViewer.SignalR
{
    public class TemperatureHub : Hub
    {
        private ISensorsAccessService sensorsAccess;
        private IHubContext<TemperatureHub> hubContext;

        public TemperatureHub(ISensorsAccessService sensorsAccess, IHubContext<TemperatureHub> hubContext)
        {
            this.sensorsAccess = sensorsAccess;
            this.hubContext = hubContext;
        }

        public void Subscribe(int[] sensors)
        {
            PersistentConnection.Instances.Add(Context.ConnectionId,
                new PersistentConnection(sensorsAccess, hubContext, sensors, Context.ConnectionId));
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                PersistentConnection persistentConnection = PersistentConnection.Instances[Context.ConnectionId];
                persistentConnection.Disconnected = true;
                PersistentConnection.Instances.Remove(Context.ConnectionId);
            }
            catch { }
            return base.OnDisconnectedAsync(exception);
        }
    }
}

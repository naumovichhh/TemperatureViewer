using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TemperatureViewer.BackgroundServices;
using TemperatureViewer.Models.DTO;

namespace TemperatureViewer.Hubs
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
            PersistentConnection.Instances.Add(Context.ConnectionId, new PersistentConnection(sensorsAccess, hubContext)
            {
                Sensors = sensors,
                ConnectionId = Context.ConnectionId
            });
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

    public class PersistentConnection
    {
        private ISensorsAccessService sensorsAccess;
        private IHubContext<TemperatureHub> hubContext;
        private Timer timer;
        private static IDictionary<string, PersistentConnection> instances = new Dictionary<string, PersistentConnection>();

        public PersistentConnection(ISensorsAccessService sensorsAccess, IHubContext<TemperatureHub> hubContext)
        {
            this.sensorsAccess = sensorsAccess;
            this.hubContext = hubContext;
            timer = new Timer(UpdateClient, null, 10000, 10000);
        }

        public static IDictionary<string, PersistentConnection> Instances
        {
            get
            {
                return instances;
            }
        }
        public bool Disconnected { get; set; }
        public int[] Sensors { get; set; }
        public string ConnectionId { get; set; }

        private void UpdateClient(object obj)
        {
            if (Disconnected)
            {
                timer.Dispose();
                return;
            }

            ValueDTO[] values = sensorsAccess.GetValues(Sensors);
            if (values != null)
            {
                var result = values.Where(v => v != null).Select(v => new { sensor = v.Sensor.Id, value = v.Temperature }).ToArray();
                hubContext.Clients.Client(ConnectionId).SendAsync("Update", result);
            }
        }

    }
}

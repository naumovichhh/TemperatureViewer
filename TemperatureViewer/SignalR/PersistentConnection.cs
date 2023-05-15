using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using TemperatureViewer.BackgroundNAccessServices;
using TemperatureViewer.Models.DTO;

namespace TemperatureViewer.SignalR
{

    public class PersistentConnection
    {
        private ISensorsAccessService sensorsAccess;
        private IHubContext<TemperatureHub> hubContext;
        private int[] sensors;
        private string connectionId;
        private Timer timer;
        private static IDictionary<string, PersistentConnection> instances = new Dictionary<string, PersistentConnection>();

        public PersistentConnection(ISensorsAccessService sensorsAccess, IHubContext<TemperatureHub> hubContext, int[] sensors, string connectionId)
        {
            this.sensorsAccess = sensorsAccess;
            this.hubContext = hubContext;
            this.sensors = sensors;
            this.connectionId = connectionId;
            timer = new Timer(UpdateClient, null, 3000, 20000);
        }

        public static IDictionary<string, PersistentConnection> Instances
        {
            get
            {
                return instances;
            }
        }
        public bool Disconnected { get; set; }

        private void UpdateClient(object obj)
        {
            if (Disconnected)
            {
                timer.Dispose();
                return;
            }

            ValueDTO[] values = sensorsAccess.GetValues(sensors, true);
            if (values != null)
            {
                var result = values.Where(v => v != null).Select(v => new { sensor = v.Sensor.Id, value = v.Temperature }).ToArray();
                hubContext.Clients.Client(connectionId).SendAsync("Update", result);
            }
        }

    }
}

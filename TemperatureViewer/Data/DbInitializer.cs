using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TemperatureViewer.Data
{
    public class DbInitializer
    {
        public static void Initialize(DefaultContext context)
        {
            context.Database.Migrate();

            if (context.Measurements.Any())
            {
                return;
            }
        }
    }
}

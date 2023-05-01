using System.Collections.Generic;

namespace TemperatureViewer.Models.ViewModels
{
    public class LocationViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }

        public IEnumerable<ValueViewModel> Values { get; set; }
    }
}

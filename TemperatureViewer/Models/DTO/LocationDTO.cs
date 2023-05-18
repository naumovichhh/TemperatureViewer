using System.Collections.Generic;

namespace TemperatureViewer.Models.DTO
{
    public class LocationDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public IEnumerable<ValueDTO> Values { get; set; }
    }
}

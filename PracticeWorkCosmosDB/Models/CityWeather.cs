using Newtonsoft.Json;
using System;


namespace PracticeWorkCosmosDB.Models
{
    public class CityInfo
    {
        [JsonProperty(PropertyName = "id")]
        public string CityId { get; set; }
        public string CityName { get; set; }

        public WeatherData Weather { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class WeatherData
    {
        public double Temperature { get; set; }
        public double Wind { get; set; }
        public double Humidity { get; set; }
        public DateTime ObservationTime { get; set; }
    }
}



using Newtonsoft.Json;

namespace PracticeWorkCosmosDB.Models
{
    public class Game
    {
        [JsonProperty("id")]
        public string GameId { get; set; }

        public string Title { get; set; }
        public string Genre { get; set; }
        public int ReleaseYear { get; set; }
        public int Rating { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

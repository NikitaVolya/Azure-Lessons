using Newtonsoft.Json;


namespace CosmoDB.Models
{
    public enum MovieGenre
    {
        Action,
        Comedy,
        Drama,
        Horror,
        SciFi,
        Romance,
        Documentary
    }

    public class Movie
    {

        [JsonProperty(PropertyName = "id")]
        public string MovieId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }

        public MovieGenre Genre { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

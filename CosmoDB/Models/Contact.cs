
using Newtonsoft.Json;

namespace CosmoDB.Models
{
    public class Contact
    {
        [JsonProperty(PropertyName = "id")]
        public string ContactId { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

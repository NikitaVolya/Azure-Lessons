

using Newtonsoft.Json;

namespace CosmoDB.Models
{
    public class Parent
    {
        public string FirstName { get; set; }
        public string FamilyName { get; set; }
    }

    public class Child
    {
        public string FirstName { get; set; }
        public string FamilyName { get; set; }
        public int Grade { get; set; }
        public int Age { get; set; }
        public Pet[] Pets { get; set; }
    }

    public class Pet
    {
        public string GivenName { get; set; }
    }

    public class Address
    {
        public string Line1 { get; set; }
        public string County { get; set; }
        public string City { get; set; }
    }

    public class Family
    {

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string LastName { get; set; }

        public Parent[] Parents { get; set; }
        public Child[] Children { get; set; }
        public Address Address { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}



using Newtonsoft.Json;
using System.Collections.Generic;

namespace PracticeWorkCosmosDB.Models
{
    public enum MenuCategory
    {
        Appetizer,
        MainCourse,
        Dessert,
        Beverage
    }

    public class MenuIngredient
    {
        public string Name { get; set; }
        public double Quantity { get; set; }
        public string Unit { get; set; }
    }

    public class MenuItem
    {
        [JsonProperty("id")]
        public string itemId { get; set; }
        public string Name { get; set; }
        public MenuCategory Category { get; set; }
        public double Price { get; set; }
        public List<MenuIngredient> Ingredients { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

using Newtonsoft.Json;

namespace Refundeo.Core.Models.Account
{
    public class FacebookUserViewModel
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("first_name")] public string FirstName { get; set; }
        [JsonProperty("last_name")] public string LastName { get; set; }
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("birthday")] public string Birthday { get; set; }
        [JsonProperty("location")] public FacebookUserLocationViewModel Location { get; set; }
    }

    public class FacebookUserLocationViewModel
    {
        [JsonProperty("location")] public FacebookLocationViewModel Location { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
    }

    public class FacebookLocationViewModel
    {
        [JsonProperty("city")] public string City { get; set; }
        [JsonProperty("country")] public string Country { get; set; }
        [JsonProperty("latitude")] public double Latitude { get; set; }
        [JsonProperty("longitude")] public double Longitude { get; set; }
        [JsonProperty("zip")] public string Zip { get; set; }
    }
}

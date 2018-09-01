using System.Collections.Generic;

namespace Refundeo.Core.Data.Models
{
    public class City
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string GooglePlaceId { get; set; }
        public Location Location { get; set; }
        public ICollection<MerchantInformation> MerchantInformations { get; set; }
    }
}

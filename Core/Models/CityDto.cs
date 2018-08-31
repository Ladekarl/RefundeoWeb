using System.Collections.Generic;
using Refundeo.Core.Models.Account;

namespace Refundeo.Core.Models
{
    public class CityDto
    {
        public string Name { get; set; }
        public string GooglePlaceId { get; set; }
        public string Image { get; set; }
        public IList<MerchantInformationRestrictedDto> Merchants { get; set; }
    }
}

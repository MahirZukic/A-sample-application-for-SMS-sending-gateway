using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using Bootstrap1.ServiceModel;
using ServiceStack.OrmLite;

namespace Bootstrap1.ServiceInterface
{
    public class CountriesService : Service
    {
        public async Task<CountriesCodePriceDetailResponse> Get(CountriesCodePriceDetailsRequest request)
        {
            return new CountriesCodePriceDetailResponse() { Result = await Db.SelectAsync<SmsCountryCodePriceDetails>() };
        }
    }
}

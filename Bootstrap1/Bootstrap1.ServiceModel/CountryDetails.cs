using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.DataAnnotations;

namespace Bootstrap1.ServiceModel
{

    [Route("/countries")]
    public class CountriesCodePriceDetailsRequest : IReturn<CountriesCodePriceDetailResponse>
    {
        // no need for parameters here
    }

    public class CountriesCodePriceDetailResponse
    {
        public List<SmsCountryCodePriceDetails> Result { get; set; }
    }

    public class SmsCountryCodePriceDetails
    {
        [PrimaryKey, AutoIncrement, Index]
        public int Id { get; set; }
        [Index]
        public String MCC { get; set; }
        public String CC { get; set; }
        public String Name { get; set; }
        public Decimal PricePerSMS { get; set; }
        public bool ShouldSerializeId()
        {
            return false;
        }
    }
}

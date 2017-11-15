using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.DataAnnotations;

namespace Bootstrap1.ServiceModel
{
    [Route("/viewstatistics")]
    public class StatisticsRequest : IReturn<StatisticsResponse>
    {
        public DateTime DateTimeFrom { get; set; }
        public DateTime DateTimeTo { get; set; }
        public String MCC { get; set; }
    }

    public class StatisticsResponse
    {
        public List<StatisticsRecordModel> Result { get; set; }
    }

    public class StatisticsRecordModel
    {
        public DateTime Day { get; set; }
        public String MCC { get; set; }
        public Decimal PricePerSMS { get; set; }
        public int Count { get; set; }
        public Decimal TotalPriceForTheDay { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack;
using Bootstrap1.ServiceModel;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Converters;

namespace Bootstrap1.ServiceInterface
{
    public class StatisticsServices : Service
    {
        public async Task<StatisticsResponse> Get(StatisticsRequest request)
        {
            List<StatisticsRecordModel> resultList = new List<StatisticsRecordModel>();
            List<String> MCCsFromRequest = !request.MCC.IsNullOrEmpty() ? request.MCC.Split(',').ToList() : null;
            List<SmsCountryCodePriceDetails> CountriesList = await Db.SelectAsync<SmsCountryCodePriceDetails>(x => MCCsFromRequest != null ? MCCsFromRequest.Contains(x.MCC) : true);
            MCCsFromRequest = MCCsFromRequest ?? CountriesList.Select(x => x.MCC).ToList();
            var tempList = (await Db.SelectAsync<SentSmsModel>(x => x.DateTime >= request.DateTimeFrom &&
                x.DateTime <= request.DateTimeTo && MCCsFromRequest.Contains(x.MCC)))
                .GroupBy(d => new { d.DateTime.Date, d.MCC })
                .Select(x =>
                            new
                            {
                                Day = x.First().DateTime.Date,
                                MCC = x.First().MCC,
                                PricePerSMS = CountriesList.FirstOrDefault(c => c.MCC.Equals(x.First().MCC)).PricePerSMS,
                                Count = x.Count(),
                                TotalPriceForTheDay = x.Sum(s => s.Price)
                            })
                .OrderBy(x => x.Day)
                .ToList();
            foreach (var DynamicObject in tempList)
            {
                resultList.Add(DynamicObject.ConvertTo<StatisticsRecordModel>());
            }
            return new StatisticsResponse { Result = resultList };
        }
    }
}

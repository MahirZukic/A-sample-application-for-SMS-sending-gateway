using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using ServiceStack;
using Bootstrap1.ServiceModel;
using Bootstrap1.ServiceInterface;
using ServiceStack.OrmLite;

namespace Bootstrap1.ServiceInterface
{
    public class SmsServices : Service
    {
        private SmsSendingService _sendingService { get; set; }

        public SmsServices(SmsSendingService sendingService)
        {
            _sendingService = sendingService;
        }
        public async Task<AlreadySentSMSResponse> Get(AlreadySentSMSRequest request)
        {
            IEnumerable<SentSmsModel> list = await
                Db.SelectAsync<SentSmsModel>(x => x.DateTime >= request.DateTimeFrom && x.DateTime <= request.DateTimeTo);
            int totalCount = list.Count();
            List<SentSmsModel> resultList = list.Skip(request.Skip ?? 0).Take(request.Take ?? Int32.MaxValue).ToList();
            return new AlreadySentSMSResponse() { TotalCount = totalCount, SentSMSes = resultList };
        }
        public async Task<SendSMSResponse> Post(SendSMSRequest request)
        {
            try
            {
                SmsModel requestQuerySmsModel = new SmsModel() {NumberFrom = request.NumberFrom, NumberTo = request.NumberTo, Text = request.Text};
                SentSmsModel sentModel = await CreateSentModelFromSmsModel(requestQuerySmsModel);
                Db.Save(sentModel);
                bool result = false;
                result = await _sendingService.Send(request.NumberFrom, request.NumberTo, request.Text);
                sentModel.State = result ? SmsSentSuccess.Success : SmsSentSuccess.Failed;
                if (sentModel.State.Equals(SmsSentSuccess.Success))
                {
                    // Only update model if there was a change to default state - Failed
                    Db.Update(sentModel);
                }
                return new SendSMSResponse() { Result = result ? SmsSentSuccess.Success : SmsSentSuccess.Failed };
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private async Task<SentSmsModel> CreateSentModelFromSmsModel(SmsModel requestQuerySmsModel)
        {
            List<SmsCountryCodePriceDetails> possibleCountries = (await Db.SelectAsync<SmsCountryCodePriceDetails>())
                .Where(x => requestQuerySmsModel.NumberTo.Replace("+", "").Replace(" ", "").StartsWith(x.CC)).ToList();
            SmsCountryCodePriceDetails countryToSendTo = possibleCountries.FirstOrDefault();
            String retrievedMcc = countryToSendTo?.MCC;
            SentSmsModel result = new SentSmsModel()
            {
                DateTime = DateTime.Now,
                MCC = retrievedMcc,
                NumberFrom = requestQuerySmsModel.NumberFrom,
                NumberTo = requestQuerySmsModel.NumberTo,
                Price = countryToSendTo?.PricePerSMS ?? 0,
                State = SmsSentSuccess.Failed,
                Text = requestQuerySmsModel.Text
            };
            return result;
        }
    }
}

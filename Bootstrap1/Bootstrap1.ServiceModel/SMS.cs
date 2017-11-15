using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.DataAnnotations;

namespace Bootstrap1.ServiceModel
{
    [Route("/sms/send")]
    public class SendSMSRequest : IReturn<SendSMSResponse>
    {
        public String NumberFrom { get; set; }
        public String NumberTo { get; set; }
        public String Text { get; set; }
    }

    [Route("/sms/sent")]
    public class AlreadySentSMSRequest : IReturn<AlreadySentSMSResponse>
    {
        public DateTime DateTimeFrom { get; set; }
        public DateTime DateTimeTo { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }

    public class SendSMSResponse
    {
        public SmsSentSuccess Result { get; set; }
    }

    public class AlreadySentSMSResponse
    {
        public int TotalCount { get; set; }
        public List<SentSmsModel> SentSMSes { get; set; }
    }

    public class SmsModel
    {
        public String NumberFrom { get; set; }
        public String NumberTo { get; set; }
        public String Text { get; set; }
        public bool ShouldSerializeText()
        {
            return false;
        }
    }

    public class SentSmsModel : SmsModel
    {
        [PrimaryKey, AutoIncrement, Index]
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        [Index]
        public String MCC { get; set; }
        public Decimal Price { get; set; }
        public SmsSentSuccess State { get; set; }
    }

    public enum SmsSentSuccess
    {
        Success,
        Failed
    }
}

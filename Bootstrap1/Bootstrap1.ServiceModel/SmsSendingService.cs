using System;
using System.Threading.Tasks;

namespace Bootstrap1.ServiceModel
{
    public interface SmsSendingService
    {
        Task<bool> Send(String numberFrom, String numberTo, String text);
    }
}
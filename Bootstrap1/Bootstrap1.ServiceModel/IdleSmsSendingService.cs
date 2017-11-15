using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bootstrap1.ServiceModel
{
    public class IdleSmsSendingService : SmsSendingService
    {
        public async Task<bool> Send(String NumberFrom, String NumberTo, String Text)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>(false);
            // get some random number between 1 and 5
            Random r = new Random();
            Double timeToSleep = r.NextDouble() * 5;

            // simulate the sending process in a real world scenario
            // in our case just sleep the amount of time previously generated in seconds
            var timer = new Timer(f => tcs.TrySetResult(true), null, (int)(timeToSleep * 1000), Timeout.Infinite);
            tcs = new TaskCompletionSource<bool>(timer);
            return await tcs.Task;
        }
    }
}
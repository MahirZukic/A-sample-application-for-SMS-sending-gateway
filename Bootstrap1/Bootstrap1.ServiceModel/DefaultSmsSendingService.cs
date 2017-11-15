using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.Configuration;

namespace Bootstrap1.ServiceModel
{
    public class DefaultSmsSendingService : SmsSendingService
    {
        public async Task<bool> Send(String numberFrom, String numberTo, String text)
        {
            AppSettings settings = new AppSettings();
            String logFileName = settings.GetString("LogFileForSms");
            bool success = await FileWriteAsync(logFileName, "{0}\r{1}\r{2}\n".FormatWith(numberFrom, numberTo, text));
            return success;
        }

        public async Task<bool> FileWriteAsync(string filePath, string messaage, bool append = true)
        {
            try
            {
                using (FileStream stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    await sw.WriteLineAsync(messaage);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
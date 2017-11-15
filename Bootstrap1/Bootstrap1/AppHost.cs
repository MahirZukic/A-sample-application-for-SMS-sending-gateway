using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Bogus;
using Funq;
using ServiceStack;
using Bootstrap1.ServiceInterface;
using Bootstrap1.ServiceModel;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Text;

namespace Bootstrap1
{
    public class AppHost : AppHostBase
    {
        /// <summary>
        /// Base constructor requires a Name and Assembly where web service implementation is located
        /// </summary>
        public AppHost()
            : base("Mitto SMS Sender", typeof(SmsServices).Assembly) { }

        public object GetInstance(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
                return Activator.CreateInstance(type);
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    return Activator.CreateInstance(type);
            }
            return null;
        }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        public override void Configure(Container container)
        {
            //Config examples
            //Plugins.Add(new PostmanFeature());
            //Plugins.Add(new CorsFeature());

            container.Register<IDbConnectionFactory>(c => new OrmLiteConnectionFactory(
//                                                         AppSettings.GetString("ConnectionString"), MySqlDialect.Provider));
                                                         AppSettings.GetString("testDb"), MySqlDialect.Provider));

            using (var db = container.Resolve<IDbConnectionFactory>().Open())
            {
                if (AppSettings.GetString("Environment") != "Production")
                {
                    // drop the table and recreate and re-seed the data for the countries table
                    db.DropTable<SmsCountryCodePriceDetails>();
                    if (db.CreateTableIfNotExists<SmsCountryCodePriceDetails>())
                    {
                        //Add seed data
                        List<SmsCountryCodePriceDetails> countriesList = new List<SmsCountryCodePriceDetails>()
                                                                         {
                                                                             new SmsCountryCodePriceDetails()
                                                                             {
                                                                                 CC = "49",
                                                                                 MCC = "262",
                                                                                 Name = "Germany",
                                                                                 PricePerSMS = new Decimal(0.055d)
                                                                             },
                                                                             new SmsCountryCodePriceDetails()
                                                                             {
                                                                                 CC = "43",
                                                                                 MCC = "232",
                                                                                 Name = "Austria",
                                                                                 PricePerSMS = new Decimal(0.053d)
                                                                             },
                                                                             new SmsCountryCodePriceDetails()
                                                                             {
                                                                                 CC = "48",
                                                                                 MCC = "260",
                                                                                 Name = "Poland",
                                                                                 PricePerSMS = new Decimal(0.032d)
                                                                             }
                                                                         };
                        db.SaveAll(countriesList);
                    }

                    // drop the table and recreate and re-seed the data for the SMS table
                    db.DropTable<SentSmsModel>();
                    if (db.CreateTableIfNotExists<SentSmsModel>())
                    {
                        List<SmsCountryCodePriceDetails> countriesList = db.Select<SmsCountryCodePriceDetails>();
                        //Add seed data
                        DateTime startingDateTime = new DateTime(2017, 11, 1);
                        DateTime endingDateTime = new DateTime(2017, 11, 20);
                        Faker<SentSmsModel> smsGenerator = new Faker<SentSmsModel>()
                            .StrictMode(true)
                            .RuleFor(s => s.Id, f => f.Random.Number())
                            .RuleFor(s => s.NumberFrom, f => f.Phone.PhoneNumber())
                            .RuleFor(s => s.NumberTo, f => f.Phone.PhoneNumber())
                            .RuleFor(s => s.Text, f => f.Lorem.Sentence())
                            .RuleFor(s => s.DateTime, f => f.Date.Between(startingDateTime, endingDateTime))
                            .RuleFor(s => s.MCC,
                                f => f.PickRandom(countriesList.Select(x => x.MCC)))
                            .RuleFor(s => s.Price, (f, i) =>
                                                   {
                                                       // pick the price corresponding to the already chosen MMC (randomly)
                                                       return countriesList.FirstOrDefault(x => x.MCC == i.MCC).PricePerSMS;
                                                   })
                            .RuleFor(s => s.State, f => f.PickRandomWithout(SmsSentSuccess.Failed));
                        List<SentSmsModel> resultList = smsGenerator.Generate(500).ToList();
                        db.SaveAll(resultList);
                    }
                }
            }

//            SmsSendingService obj = (SmsSendingService) GetInstance("Bootstrap1.ServiceModel.DefaultSmsSendingService");
//            container.RegisterAutoWiredAs<(castedObject.GetType(), SmsSendingService>();
            container.RegisterAutoWiredAs<DefaultSmsSendingService, SmsSendingService>();
//            container.RegisterAutoWiredAs<IdleSmsSendingService, SmsSendingService>();

            JsConfig<DateTime>.SerializeFn = time => new DateTime(time.Ticks, DateTimeKind.Local).ToString("o");
            JsConfig<DateTime?>.SerializeFn =
                time => time != null ? new DateTime(time.Value.Ticks, DateTimeKind.Local).ToString("o") : null;
            JsConfig.DateHandler = DateHandler.ISO8601;


            Feature featuresToEnable = Feature.Json | Feature.Xml;
            if (AppSettings.GetString("Environment") != "Production")
            {
                featuresToEnable |= Feature.Metadata;
            }
            // Enable only JSON and XML features (use Metadata for development, remove in production)
            HostConfig.Instance.EnableFeatures = featuresToEnable;

            Plugins.Add(new TemplatePagesFeature());
        }
    }
}
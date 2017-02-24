using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using MessageHandler.Runtime;
using MessageHandler.Runtime.ConfigurationSettings;
using MessageHandler.Runtime.EventProcessing;
using MessageHandler.Runtime.EventProcessing.Convention;
using Microsoft.ServiceBus.Messaging;

namespace EventHubSender
{
    class Program
    {
        private static readonly HandlerRuntimeConfiguration config = new HandlerRuntimeConfiguration();
        private static readonly ISettings settings = config.GetSettings();
        static void Main(string[] args)
        {
            try
            {
                MainAsync(args).Wait();
            }
            catch (AggregateException ex)
            {
                ExceptionDispatchInfo.Capture(ex.Flatten().InnerExceptions.First()).Throw();
            }
        }
        static async Task MainAsync(string[] args)
        {
            try
            {
                config.Connectionstring(Environment.GetEnvironmentVariable("MessageHandler.EventHub.Connectionstring"));
                config.UseEventProcessingRuntime();
                config.ChannelId("consoleEventHub");
                config.DisruptorRingSize(1024);
                config.HandlerConfigurationId("test");
                var client = EventHubClient.CreateFromConnectionString(settings.GetConnectionstring(), settings.GetChannelId());
                bool YN= false;
                do
                {
                    
                        
                    await client.SendAsync(new EventData(Encoding.UTF8.GetBytes("test message")));
                } while (YN == false); 
                Console.WriteLine("Press a key to stop.");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }
    }
}

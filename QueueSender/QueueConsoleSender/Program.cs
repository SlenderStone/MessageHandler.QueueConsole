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
using MessageHandler.Runtime.EventProcessing.MessagePump.Pumps;
using Microsoft.ServiceBus.Messaging;

namespace QueueConsoleSender
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
                config.Connectionstring(
                    Environment.GetEnvironmentVariable("MessageHandler.AzureServiceBus.Connectionstring"));
                config.ChannelId("Console");
                config.DisruptorRingSize(1024);
                var pump = new QueuePump(settings);
                var messageReceiverSettings = new MessageReceiverSettings()
                {
                    NumberOfReceivers = 5,
                    BatchSize = 100,
                    ServerWaitTime = TimeSpan.FromSeconds(1)
                };
                config.MessageReceiverSettings(messageReceiverSettings);
                config.RegisterMessagePump(pump);
                config.UseEventProcessingRuntime();
                Func<IProcessingContext, Task> pipeline = ctx => Task.CompletedTask;
                config.Pipeline(pipeline);
                Console.WriteLine("Press a key to start.");
                Console.ReadKey();
                bool YN = false;
                do
                {
                    await SendMessage();
                    if (DateTime.Now.Minute == 32)
                        YN = true;
                } while (YN == false); 
                Console.WriteLine("Messages sent.");
                Console.ReadKey();
                Console.WriteLine("Program finished.");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }

        public static async Task SendMessage()
        {
            var factory = MessagingFactory.CreateFromConnectionString(settings.GetConnectionstring());
            List<BrokeredMessage> messages = new List<BrokeredMessage>();
            var myMessageSender = factory.CreateMessageSender(settings.GetChannelId());
            for (int i = 0; i < 100; i++)
            {
                messages.Clear();
                for (int j = 0; j < 300; j++)
                {
                    var message = new BrokeredMessage("Console single message");
                    messages.Add(message);
                }
                await myMessageSender.SendBatchAsync(messages);
            }
        }
    }
}

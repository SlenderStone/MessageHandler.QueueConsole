using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using MessageHandler.Runtime;
using MessageHandler.Runtime.ConfigurationSettings;
using MessageHandler.Runtime.EventProcessing;
using MessageHandler.Runtime.EventProcessing.Convention;
using MessageHandler.Runtime.EventProcessing.MessagePump.Pumps;
using Microsoft.ServiceBus.Messaging;

namespace QueueConsole
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
                    NumberOfReceivers = 20,
                    BatchSize = 1000,
                    ServerWaitTime = TimeSpan.FromMilliseconds(1000)
                };
                config.MessageReceiverSettings(messageReceiverSettings);
                config.RegisterMessagePump(pump);
                config.UseEventProcessingRuntime();
                Func<IProcessingContext, Task> pipeline = ctx => Task.CompletedTask;
                config.Pipeline(pipeline);
                var runtime = await HandlerRuntime.Create(config);
                await runtime.Start();
                Console.WriteLine("Press a key to stop.");
                Console.ReadKey();
                await runtime.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }
    }
}

﻿using System;
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

namespace EventHubReceiver
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

                var leaseStore = new InMemoryLeaseStore<EventHubPump.CheckpointManagerLease>(new EventHubPump.CheckpointManagerLeaseCreator());
                var leaseAllocation = new InMemoryLeaseAllocation<EventHubPump.CheckpointManagerLease>(leaseStore, new EventHubPump.CheckpointManagerLeaseCreator());
                var pump = new EventHubPump(settings, leaseAllocation, leaseStore);
                config.RegisterMessagePump(pump);
                int invocationCount = 0;

                Func<IProcessingContext, Task> pipeline = async ctx =>
                {
                    await Console.Out.WriteLineAsync(invocationCount++ +" "+DateTime.Now.ToLongTimeString());
                    Console.WriteLine();
                };
                config.Pipeline(pipeline);

                var runtime = await HandlerRuntime.Create(config);
                await runtime.Start();

                await leaseAllocation.Allocate();
                
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

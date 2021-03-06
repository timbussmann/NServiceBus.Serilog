﻿using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Serilog;
using NServiceBus.Serilog.Tracing;
using Serilog;

class Program
{
    static async Task Main()
    {
        var tracingLog = new LoggerConfiguration()
            .WriteTo.Seq("http://localhost:5341")
            .MinimumLevel.Information()
            .CreateLogger();
        //Set NServiceBus to log to Serilog
        var serilogFactory = LogManager.Use<SerilogFactory>();
        serilogFactory.WithLogger(tracingLog);

        var configuration = new EndpointConfiguration("SeqSample");
        configuration.EnableFeature<TracingLog>();
        configuration.SerilogTracingTarget(tracingLog);
        configuration.EnableInstallers();
        configuration.UsePersistence<InMemoryPersistence>();
        configuration.UseTransport<LearningTransport>();
        configuration.SendFailedMessagesTo("error");
        var endpoint = await Endpoint.Start(configuration)
            .ConfigureAwait(false);
        var createUser = new CreateUser
        {
            UserName = "jsmith",
            FamilyName = "Smith",
            GivenNames = "John",
        };
        await endpoint.SendLocal(createUser)
            .ConfigureAwait(false);
        await endpoint.ScheduleEvery(TimeSpan.FromSeconds(1), context => context.SendLocal(createUser));
        Console.WriteLine("Press any key to stop program");
        Console.Read();
    }
}
using System;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using AkkaPocUtils;

namespace DistributePubSubExample.JobHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "DistributePubSub.JobHandler";
            //var config = DistributedPubSub.DefaultConfig().WithFallback(HoconLoader.ParseDefaultConfig());
            var config = HoconLoader.ParseDefaultConfig().WithFallback(DistributedPubSub.DefaultConfig());
            var system = ActorSystem.Create("mirerosystem", config);
            //system.UseSerilog();

            //Console.WriteLine(system.Settings.Config.GetConfig("akka.actor.serializers"));
            //Console.WriteLine("Press any key to continue");
            //Console.ReadLine();
            
            var echo = system.ActorOf(Props.Create(() => new JobHandlerActor()), "handler");
            echo.Tell(new object());
            
            system.Run();
        }
    }
}
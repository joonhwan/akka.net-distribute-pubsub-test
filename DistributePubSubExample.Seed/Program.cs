using System;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Tools.PublishSubscribe;
using AkkaPocUtils;

namespace SimpleSeedNode
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Seed";
            
            var config = HoconLoader.ParseDefaultConfig().WithFallback(DistributedPubSub.DefaultConfig());
            
            var system = ActorSystem.Create("mirerosystem", config);
            //system.UseSerilog();

            DistributedPubSub.Get(system); // --> DistributedPubsub이 만들어진다.

            system.Run();
        }
    }
}

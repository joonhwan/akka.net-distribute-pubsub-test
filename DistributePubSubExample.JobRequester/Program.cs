using System;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using AkkaPocUtils;
using DistributePubSubExample.Shared;

namespace DistributePubSubExample.JobRequester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "DistributePubSub.JobRequester";
            var config = HoconLoader.ParseDefaultConfig().WithFallback(DistributedPubSub.DefaultConfig());
            var system = ActorSystem.Create("mirerosystem", config);
            
            var generator = system.ActorOf(Props.Create<JobRequesterActor>(), "distributor");
            
            system.Run(wait: false);

            Console.WriteLine("any alphabet will be published in 1-to-n way");
            Console.WriteLine("any number will be published in 1-to-1 way");
            Console.WriteLine("'q' to quit...");
            var input = Console.ReadLine();
            while (input != "q")
            {
                var job = new RequestJob(input);
                generator.Tell(job);
                input = Console.ReadLine();
            }
        }
    }

}

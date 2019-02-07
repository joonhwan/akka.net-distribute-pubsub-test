using System;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using DistributePubSubExample.Shared;

namespace DistributePubSubExample.JobHandler
{
    internal class JobHandlerActor : UntypedActor
    {
        public const string Topic = "echo";

        public readonly Akka.Cluster.Cluster Cluster = Akka.Cluster.Cluster.Get(Context.System);
        public readonly IActorRef Mediator = DistributedPubSub.Get(Context.System).Mediator;

        private ILoggingAdapter _log = Context.GetLogger();

        protected override void PreStart()
        {
            base.PreStart();

            Console.Title = $"JobHandler:{Cluster.SelfAddress}";
            Console.WriteLine("JobHandler Actor Started : {0}", this.Self.Path);
            Mediator.Tell(new Subscribe(Topic, Self, "handler"));
            Mediator.Tell(new Subscribe(Topic, Self));
            //Mediator.Tell(new Subscribe(Topic, Self, $"jobhandler-group-{Guid.NewGuid().ToString("N")}"));
        }

        protected override void PostStop()
        {
            Mediator.Tell(new Unsubscribe(Topic, Self));
            base.PostStop();
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case SubscribeAck ack:
                    Console.WriteLine("Actor [{0}] has subscribed to topic [{1}]", ack.Subscribe.Ref, ack.Subscribe.Topic);
                    break;
                case RequestJob echo:
                    Console.WriteLine("Received : {0}", echo.Content);
                    break;
            }
        }
    }
}
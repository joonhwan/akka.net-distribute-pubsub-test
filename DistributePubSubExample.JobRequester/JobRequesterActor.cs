//#define SCHEDULED_JOB
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using DistributePubSubExample.Shared;

namespace DistributePubSubExample.JobRequester
{
    class JobRequesterActor : UntypedActor
    {
        public readonly Akka.Cluster.Cluster Cluster = Akka.Cluster.Cluster.Get(Context.System);
        public readonly IActorRef Mediator = DistributedPubSub.Get(Context.System).Mediator;
        private HashSet<UniqueAddress> _upMembers;
        private readonly ILoggingAdapter _log = Context.GetLogger();

        protected override void PreStart()
        {
            base.PreStart();

            Console.Title = $"JobRequester:{Cluster.SelfAddress}";

            Cluster.Subscribe(Self, ClusterEvent.InitialStateAsEvents, 
                typeof(ClusterEvent.IMemberEvent), 
                typeof(ClusterEvent.UnreachableMember)
            );

            _upMembers = new HashSet<UniqueAddress>();
            BecomeNoJobHandlerAvailableState();
        }

        protected override void PostStop()
        {
            Cluster.Unsubscribe(Self);
            base.PostStop();
        }

        private void BecomeNoJobHandlerAvailableState()
        {
            Console.WriteLine("Become No-JobHandler-AvailableState");
            Become(NoJobHandlerAvailableState);
        }

        private void NoJobHandlerAvailableState(object message)
        {
            switch (message)
            {
                case ClusterEvent.MemberUp e:
                    ProcessMemberAdd(e.Member);
                    if (_upMembers.Count > 0)
                    {
                        BecomeJobHandlerAvailableState();
                    }
                    break;
            }
        }

        private void BecomeJobHandlerAvailableState()
        {
            Console.WriteLine("Become JobHandler-AvailableState");
#if SCHEDULED_JOB
            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(1),  Self, "GenerateJob", Self);
#endif
            Become(JobHandlerAvailableState);
        }

        private int _counter = 0;
        private void JobHandlerAvailableState(object message)
        {
            switch (message)
            {
                case "GenerateJob":
                    var job = new RequestJob($"GeneratedJob[{_counter}] @ {DateTime.Now:HH:mm:ss.tt}");
                    
                    //Mediator.Tell(new Publish("echo", job));
                    Mediator.Tell(new Publish("echo", job, sendOneMessageToEachGroup: true));

#if SCHEDULED_JOB
                    Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(1), Self, "GenerateJob", Self);
#endif
                    Console.WriteLine("Generated New Job : '{0}'", job.Content);
                    _counter++;
                    break;
                case RequestJob requestedJob:
                    var content = requestedJob.Content;
                    var sendOneMessageToEachGroup = content.Length > 0 && "0123456789".Contains(content[0]);
                    Console.WriteLine(">> Publishing Request : {0}", sendOneMessageToEachGroup ? "1-to-1" : "1-to-n");
                    Mediator.Tell(new Publish("echo", requestedJob, sendOneMessageToEachGroup));
                    break;
                case ClusterEvent.MemberUp e:
                    ProcessMemberAdd(e.Member);
                    break;
                case ClusterEvent.MemberRemoved e:
                    //Console.WriteLine("MemberRemoved : {0}", e.Member);
                    ProcessMemberRemoval(e.Member);
                    break;
                case ClusterEvent.MemberLeft e:
                    //Console.WriteLine("MemberLeft : {0}", e.Member);
                    ProcessMemberRemoval(e.Member);
                    break;
                case ClusterEvent.MemberExited e:
                    //Console.WriteLine("MemberExited : {0}", e.Member);
                    ProcessMemberRemoval(e.Member);
                    break;
                case ClusterEvent.UnreachableMember e:
                    //Console.WriteLine("UnreachableMember : {0}", e.Member);
                    ProcessMemberRemoval(e.Member);
                    break;
            }
        }

        private void ProcessMemberAdd(Member m)
        {
            //Console.WriteLine("MemberUp : {0}", m);
            if (m.IsJobHandlerMember())
            {
                _upMembers.Add(m.UniqueAddress);
            }
        }

        private void ProcessMemberRemoval(Member m)
        {
            if (m.IsJobHandlerMember())
            {
                _upMembers.Remove(m.UniqueAddress);
                if(_upMembers.Count == 0)
                {
                    BecomeNoJobHandlerAvailableState();
                }
            }
        }

        protected override void OnReceive(object message)
        {
            // not used
        }
    }
    
    public static class MemberExtensions
    {
        public static bool IsJobHandlerMember(this Member m)
        {
            return m?.HasRole("handler") ?? false;
        }
    }
}
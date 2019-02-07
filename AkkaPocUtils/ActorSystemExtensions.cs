using System;
using System.Threading;
using Akka.Actor;

namespace AkkaPocUtils
{
    public static class ActorSystemExtensions
    {
        public static void Run(this ActorSystem system, bool wait = true)
        {
            Console.WriteLine("Press Control + C to terminate.");
            var shutDowned = new ManualResetEvent(false);
            CoordinatedShutdown.Get(system);
            AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
            {
                Console.WriteLine("Process Exiting..."); 
            };

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                shutDowned.Set();
            };
            if (wait)
            {
                shutDowned.WaitOne();
            }

            // 아래와 같이 Actor System을 중단하는 것은 필요없다(ie: CoordinatedShutDown!!! 이 자동으로 한다. ProcessExit핸들러에서...)
            // 
            //system.Terminate().Wait();
        }
    }
}
namespace DistributePubSubExample.Shared
{
    public class RequestJob
    {
        public RequestJob(string content)
        {
            Content = content;
        }

        public string Content { get; }
    }
}

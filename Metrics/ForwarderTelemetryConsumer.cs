
using Yarp.ReverseProxy.Forwarder;
using Yarp.Telemetry.Consumption;

namespace MyProxy.Metrics
{
    public sealed class ForwarderTelemetryConsumer : IForwarderTelemetryConsumer
    {
        private static Dictionary<string, long> Bandwidth {get; set;} = new Dictionary<string, long>();
        public void OnForwarderStart(DateTime timestamp, string destinationPrefix)
        {
            var metrics = PerRequestMetrics.Current;
            metrics.ProxyStartOffset = metrics.CalcOffset(timestamp);
            metrics.DestinationPrefix = destinationPrefix;
        }

        public void OnForwarderStop(DateTime timestamp, int statusCode)
        {
            var metrics = PerRequestMetrics.Current;
            metrics.ProxyStopOffset = metrics.CalcOffset(timestamp);
        }

        public void OnForwarderFailed(DateTime timestamp, ForwarderError error)
        {
            var metrics = PerRequestMetrics.Current;
            metrics.Error = error;
        }

        public void OnContentTransferred(DateTime timestamp, bool isRequest, long contentLength, long iops, TimeSpan readTime, TimeSpan writeTime, TimeSpan firstReadTime)
        {
            var metrics = PerRequestMetrics.Current;

            if (isRequest)
            {
                metrics.RequestBodyLength = contentLength;
                metrics.RequestContentIops = iops;
            }
            else
            {
                // We don't get a content stop from http as it is returning a stream that is up to the consumer to
                // read, but we know its ended here.
                metrics.HttpResponseContentStopOffset = metrics.CalcOffset(timestamp);
                metrics.ResponseBodyLength = contentLength;
                metrics.ResponseContentIops = iops;
            }
            if (Bandwidth.ContainsKey(metrics.DestinationId))
            {
                Bandwidth[metrics.DestinationId]+=contentLength;
            }
            else
            {
                Bandwidth.Add(metrics.DestinationId, contentLength);
            }
            var bandwidth = Bandwidth[metrics.DestinationId];
            Console.WriteLine($"Stats: {metrics.DestinationPrefix} {metrics.DestinationId} {metrics.ResponseBodyLength} {bandwidth}");
        }

        public void OnForwarderInvoke(DateTime timestamp, string clusterId, string routeId, string destinationId)
        {
            var metrics = PerRequestMetrics.Current;
            metrics.RouteInvokeOffset = metrics.CalcOffset(timestamp);
            metrics.RouteId = routeId;
            metrics.ClusterId = clusterId;
            metrics.DestinationId = destinationId;
        }
    }
}
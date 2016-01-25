namespace System.Net.Http
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class TestHttpEndpoint
    {
        private Lazy<HttpMessageHandler> _handler;

        public string Url { get; private set; }
        public HttpMessageHandler MessageHandler => _handler.Value;

        public TestHttpEndpoint(string url = null, HttpMessageHandler messageHandler = null)
        {
            Url = url ?? "http://127.0.0.1/";
            _handler = new Lazy<HttpMessageHandler>(() => messageHandler);
        }

        public TestHttpEndpoint(string url, Func<HttpMessageHandler> messageHandler)
        {
            Url = url ?? "http://127.0.0.1/";
            _handler = new Lazy<HttpMessageHandler>(messageHandler);
        }

        public override string ToString() => Url;

        public static IEnumerable<TestHttpEndpoint> FromFile(string endpointFile = "Endpoints")
        {
            if (!File.Exists(endpointFile)) return Enumerable.Empty<TestHttpEndpoint>();

            return from line in File.ReadAllLines(endpointFile)
                   let trim = line.Trim()
                   where !trim.StartsWith("//")
                   select new TestHttpEndpoint(trim);
        }

        public static TheoryData<TestHttpEndpoint> CreateTheoryData(params TestHttpEndpoint[] additonalEndpoints)
        {
            var result = new TheoryData<TestHttpEndpoint>();
            foreach (var e in FromFile())
            {
                result.Add(e);
            }
            foreach (var e in additonalEndpoints)
            {
                result.Add(e);
            }
            return result;
        }
    }

    public static class TestHttpEndpointExtensions
    {
        public static TestHttpEndpoint Simulate(this TestHttpEndpoint endpoint, string urlOverride = null)
        {
            return new TestHttpEndpoint(urlOverride ?? endpoint.Url, () =>
            {
                return endpoint.MessageHandler as TestHttpMessageHandler ?? new TestHttpMessageHandler(endpoint.MessageHandler);
            });
        }

        public static TestHttpEndpoint Latency(this TestHttpEndpoint endpoint, TimeSpan latency)
        {
            var test = endpoint.MessageHandler as TestHttpMessageHandler;
            if (test == null)
            {
                endpoint = new TestHttpEndpoint(endpoint.Url, test = new TestHttpMessageHandler(endpoint.MessageHandler));
            }
            test.Latency = latency;
            return endpoint;
        }

        public static TestHttpEndpoint Predicate(this TestHttpEndpoint endpoint, Func<HttpRequestMessage, bool> predicate)
        {
            var test = endpoint.MessageHandler as TestHttpMessageHandler;
            if (test == null)
            {
                endpoint = new TestHttpEndpoint(endpoint.Url, test = new TestHttpMessageHandler(endpoint.MessageHandler));
            }
            test.Predicate = predicate;
            return endpoint;
        }

        public static TestHttpEndpoint Connected(this TestHttpEndpoint endpoint, bool connected)
        {
            return Connected(endpoint, connected ? 1 : 0);
        }

        public static TestHttpEndpoint Connected(this TestHttpEndpoint endpoint, double networkAvailibility)
        {
            var test = endpoint.MessageHandler as TestHttpMessageHandler;
            if (test == null)
            {
                endpoint = new TestHttpEndpoint(endpoint.Url, test = new TestHttpMessageHandler(endpoint.MessageHandler));
            }
            test.NetworkAvailibility = networkAvailibility;
            return endpoint;
        }
    }

    class TestHttpMessageHandler : DelegatingHandler
    {
        private static Random random = new Random();
        private bool once;

        public TimeSpan Latency { get; set; } = TimeSpan.Zero;
        public double NetworkAvailibility { get; set; } = 1;
        public HttpStatusCode? StatusCode { get; set; }
        public Func<HttpRequestMessage, bool> Predicate { get; set; }

        public TestHttpMessageHandler() { }
        public TestHttpMessageHandler(HttpMessageHandler handler) : base(handler ?? new HttpClientHandler()) { }

        public void SetStatusCodeOnce(HttpStatusCode code, Func<HttpRequestMessage, bool> predicate = null)
        {
            StatusCode = code;
            once = true;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (Predicate != null && !Predicate(request))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            if (StatusCode.HasValue)
            {
                var response = new HttpResponseMessage(StatusCode.Value);
                if (once) { once = false; StatusCode = null; }
                return response;
            }

            await Task.Delay(random.Next((int)Latency.TotalMilliseconds / 2) + (int)Latency.TotalMilliseconds);

            if (random.NextDouble() > NetworkAvailibility)
            {
                throw new HttpRequestException("Faked exception from " + typeof(TestHttpMessageHandler).Name);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
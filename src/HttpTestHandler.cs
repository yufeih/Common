namespace System.Net.Http
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    class HttpTestHandler : DelegatingHandler
    {
        private static Random random = new Random();
        private bool once;

        public TimeSpan Latency { get; set; } = TimeSpan.Zero;
        public double NetworkAvailibility { get; set; } = 1;
        public HttpStatusCode? StatusCode { get; set; }
        public Func<HttpRequestMessage, bool> Predicate { get; set; }

        public HttpTestHandler() { }
        public HttpTestHandler(HttpMessageHandler handler) : base(handler ?? new HttpClientHandler()) { }

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
                throw new HttpRequestException("Faked exception from " + typeof(HttpTestHandler).Name);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
namespace System.Net.Http
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    static class HttpHelper
    {
        public static string ValidatePath(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (path == "" || path == "/") return "";
            if (path[0] != '/' || path[path.Length - 1] == '/') throw new ArgumentException(nameof(path));

            return path;
        }

        public static DelegatingHandler Combine(this IEnumerable<DelegatingHandler> handlers)
        {
            var result = handlers.FirstOrDefault();
            var last = result;

            foreach (var handler in handlers.Skip(1))
            {
                last.InnerHandler = handler;
                last = handler;
            }

            return result;
        }

        [Conditional("DEBUG")]
        public static void DumpErrors(HttpResponseMessage message)
        {
            if (!message.IsSuccessStatusCode && message.StatusCode != HttpStatusCode.Unauthorized)
            {
                var dump = (Action)(async () =>
                {
                    var error = await message.Content.ReadAsStringAsync();
                    Debug.WriteLine(error);
                });
                dump();
            }
        }

        public static HttpResponseMessage EnsureSuccess(this HttpResponseMessage message)
        {
            if (message.IsSuccessStatusCode) return message;

            throw new HttpRequestException($"Http {message.StatusCode}: {message.Content.ReadAsStringAsync().Result}");
        }

        public static int TryGetContentLength(HttpResponseMessage response)
        {
            int result;
            IEnumerable<string> value;
            if (response.Headers.TryGetValues("Content-Length", out value) &&
                value.Any() && int.TryParse(value.First(), out result))
            {
                return result;
            }
            return 0;
        }
    }
}

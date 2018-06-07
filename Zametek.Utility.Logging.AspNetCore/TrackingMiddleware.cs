using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Zametek.Utility.Logging.AspNetCore
{
    public class TrackingMiddleware
    {
        public const string RemoteIpAddressName = nameof(ConnectionInfo.RemoteIpAddress);
        public const string TraceIdentifierName = nameof(HttpContext.TraceIdentifier);
        public const string UserIdName = @"UserId";
        private readonly RequestDelegate m_Next;
        private readonly Func<HttpContext, IDictionary<string, string>> m_SetupFunc;

        public TrackingMiddleware(RequestDelegate next)
            : this(next, _ => new Dictionary<string, string>())
        {
        }

        public TrackingMiddleware(RequestDelegate next, Func<IDictionary<string, string>> setupFunc)
        {
            if (setupFunc == null)
            {
                throw new ArgumentNullException(nameof(setupFunc));
            }
            m_Next = next ?? throw new ArgumentNullException(nameof(next));
            m_SetupFunc = _ => setupFunc.Invoke();
        }

        public TrackingMiddleware(RequestDelegate next, Func<HttpContext, IDictionary<string, string>> setupFunc)
        {
            m_Next = next ?? throw new ArgumentNullException(nameof(next));
            m_SetupFunc = setupFunc ?? throw new ArgumentNullException(nameof(setupFunc));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            IDictionary<string, string> extraHeaders = m_SetupFunc?.Invoke(httpContext) ?? new Dictionary<string, string>();
            Debug.Assert(extraHeaders != null);
            TrackingContext.NewCurrentIfEmpty(extraHeaders);

            using (LogContext.Push(new TrackingContextEnricher()))
            using (LogContext.PushProperty(RemoteIpAddressName, httpContext.Connection.RemoteIpAddress))
            using (LogContext.PushProperty(TraceIdentifierName, httpContext.TraceIdentifier))
            using (LogContext.PushProperty(UserIdName, httpContext.User?.Identity?.Name))
            {
                // Must await the next middlware, otherwise the log context will unwind at the first asyncronious
                // operation.
                await m_Next.Invoke(httpContext).ConfigureAwait(false);
            }
        }
    }
}

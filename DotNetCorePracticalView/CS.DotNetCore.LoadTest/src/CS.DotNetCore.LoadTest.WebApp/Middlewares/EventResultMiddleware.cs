namespace CS.DotNetCore.LoadTest.WebApp.Middlewares
{
    using Data;
    using Logging;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Util;

    internal class EventResultMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IEventResultDAO _eventResultDAO;
        private readonly ILogger<EventResultMiddleware> _logger;

        public EventResultMiddleware(RequestDelegate next, IEventResultDAO eventResultDAO, ILoggerFactory loggerFactory)
        {
            _next = next;
            _eventResultDAO = eventResultDAO;
            _logger = loggerFactory.CreateLogger<EventResultMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            var stopWatch = new Stopwatch();
            long elapsedTime = 0;

            var eventStart = DateTimeOffset.UtcNow;
            Event eventObject = null;

            try
            {
                //executing service
                stopWatch.Start();
                await _next(context).ConfigureAwait(false);
                stopWatch.Stop();

                //resolving status code and duration
                elapsedTime = stopWatch.ElapsedMilliseconds;
            }
            catch (Exception e)
            {
                //stopping stopwatch and resolving event
                if (stopWatch.IsRunning)
                {
                    stopWatch.Stop();
                }

                eventObject = context.GetEvent();

                if (eventObject == null)
                {
                    eventObject = Event.CreateNone();
                }

                elapsedTime = stopWatch.ElapsedMilliseconds;

                //logging event error
                _logger.LogError(eventObject.EventId, e, null, eventObject.EventInputs);
                context.Response.StatusCode = 500;
            }
            finally
            {
                //resolving event
                if (eventObject == null)
                {
                    eventObject = context.GetEvent();
                }

                //saving event result
                if (eventObject != null && Event.None.Id != eventObject.EventId.Id)
                {
                    var eventResult = new EventResult(eventObject, eventStart, elapsedTime,
                        context.Response.StatusCode, context.Request.GetTest());

                    _eventResultDAO.Insert(eventResult);
                    eventResult = null;
                }

                //returning public error msg
                if (context.Response.StatusCode == 500)
                {
                    await context.Response.WriteAsync("Internal Server Error.").ConfigureAwait(false);
                }

                eventObject = null;
            }
        }
    }
}

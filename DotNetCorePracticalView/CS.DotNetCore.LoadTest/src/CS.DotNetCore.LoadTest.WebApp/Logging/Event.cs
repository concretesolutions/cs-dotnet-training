namespace CS.DotNetCore.LoadTest.WebApp.Logging
{
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Linq;

    internal class Event
    {
        internal static readonly EventId None = new EventId(0, "None");
        internal static readonly EventId PostIdentityMemory = new EventId(1, "PostIdentityMemory");
        internal static readonly EventId PostIdentityDb = new EventId(2, "PostIdentityDb");

        [JsonProperty]
        public object[] EventInputs { get; private set; }

        [JsonProperty]
        public EventId EventId { get; private set; }

        [JsonConstructor]
        private Event() { }

        internal Event(EventId eventId, IEnumerable<object> eventInputs = null)
        {
            EventId = eventId;
            EventInputs = eventInputs == null ? new object[0] : eventInputs.ToArray();
        }

        public static Event CreatePostIdentityMemory(IEnumerable<object> eventInputs = null)
        {
            return new Event(PostIdentityMemory, eventInputs);
        }

        public static Event CreatePostIdentityDb(IEnumerable<object> eventInputs = null)
        {
            return new Event(PostIdentityDb, eventInputs);
        }

        public static Event CreateNone()
        {
            return new Event(None);
        }

        public static int GetExpectedStatusCode(EventId eventId)
        {
            if (PostIdentityDb.Id == eventId.Id || PostIdentityMemory.Id == eventId.Id)
            {
                return 201;
            }

            return 200;
        }
    }
}

namespace CS.DotNetCore.LoadTest.WebApp.Data.Memory
{
    using Logging;
    using System.Collections.Concurrent;
    using System;
    using System.Linq;
    using System.Collections.Generic;

    internal class EventResultMemoryDAO : IEventResultDAO
    {
        private static readonly ConcurrentBag<EventResult> _dataBase = new ConcurrentBag<EventResult>();

        public void Insert(EventResult eventResult)
        {
            if (eventResult == null)
                throw new ArgumentNullException(nameof(eventResult));

            _dataBase.Add(eventResult);
        }

        public List<EventResult> SelectAll() {

            lock (_dataBase)
            {
                return _dataBase.ToList();
            }
        }

        public int DeleteAll()
        {
            lock (_dataBase)
            {
                var affected = _dataBase.Count;

                if (affected > 0)
                {
                    EventResult eventResult;

                    while (!_dataBase.IsEmpty)
                    {
                        _dataBase.TryTake(out eventResult);
                    }
                }

                return affected;
            }
        }
    }
}

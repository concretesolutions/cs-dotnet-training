namespace CS.DotNetCore.LoadTest.WebApp.Data.Memory
{
    using Business;
    using System;
    using System.Collections.Concurrent;

    internal class IdentityMemoryDAO : IIdentityDAO
    {
        private static readonly ConcurrentDictionary<string, Identity> _dataBase = new ConcurrentDictionary<string, Identity>();

        public void Insert(Identity identity)
        {
            if (!_dataBase.TryAdd(identity.IdentityName, identity))
            {
                throw new Exception(string.Concat("insertion for identity \"", identity.IdentityName, "\" fails"));
            }
        }

        public int DeleteAll()
        {
            lock (_dataBase)
            {
                var affected = _dataBase.Count;
                _dataBase.Clear();

                return affected;
            }
        }
    }
}

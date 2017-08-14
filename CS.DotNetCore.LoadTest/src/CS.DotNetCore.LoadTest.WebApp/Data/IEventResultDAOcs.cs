namespace CS.DotNetCore.LoadTest.WebApp.Data
{
    using Logging;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEventResultDAO
    {
        void Insert(EventResult eventResult);

        List<EventResult> SelectAll();

        int DeleteAll();
    } 
}

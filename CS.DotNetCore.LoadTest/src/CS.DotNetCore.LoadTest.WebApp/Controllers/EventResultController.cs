namespace CS.DotNetCore.LoadTest.WebApp.Controllers
{
    using Data;
    using Microsoft.AspNetCore.Mvc;

    public class EventResultController : Controller
    {
        private readonly IEventResultDAO _eventResultDAO;
        
        public EventResultController(IEventResultDAO eventResultDAO)
        {
            _eventResultDAO = eventResultDAO;
        }

        [HttpDelete]
        [Route("api/v1/event_result/all")]
        public IActionResult DeleteAllEventResultsFromMemory()
        {
            _eventResultDAO.DeleteAll();
            return Ok();
        }
    }
}

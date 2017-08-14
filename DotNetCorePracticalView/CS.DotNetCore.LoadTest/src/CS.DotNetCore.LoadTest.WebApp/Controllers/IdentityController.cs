namespace CS.DotNetCore.LoadTest.WebApp.Controllers
{
    using Business;
    using Config;
    using Data;
    using Logging;
    using Microsoft.AspNetCore.Mvc;
    using ServiceModel;
    using System.Threading.Tasks;
    using Util;

    public class IdentityController : Controller
    {
        private readonly ILoadTestConfig _config;
        private readonly IIdentityDAO _identityDAO;
        private readonly IIdentityAsyncDAO _identityAsyncDAO;

        public IdentityController(ILoadTestConfig configuration, IIdentityDAO identityDAO, IIdentityAsyncDAO identityAsyncDAO)
        {
            _config = configuration;
            _identityDAO = identityDAO;
            _identityAsyncDAO = identityAsyncDAO;
        }

        [HttpPost]
        [Route("api/v1/identity/memory")]
        public IActionResult PostIdentityMemory([FromBody]PostIdentityRequest request)
        {
            HttpContext.AddEvent(Event.CreatePostIdentityMemory(new object[1] { request }));
            var identity = new Identity(request.IdentityName);

            identity.SetPassword(request.Password, _config);
            _identityDAO.Insert(identity);

            return StatusCode(201);
        }

        [HttpPost]
        [Route("api/v1/identity/db")]
        public async Task<IActionResult> PostIdentityDbAsync([FromBody]PostIdentityRequest request)
        {
            HttpContext.AddEvent(Event.CreatePostIdentityDb(new object[1] { request }));
            var identity = new Identity(request.IdentityName);

            identity.SetPassword(request.Password, _config);
            await _identityAsyncDAO.InsertAsync(identity).ConfigureAwait(false);

            return StatusCode(201);
        }

        [HttpDelete]
        [Route("api/v1/identity/all/memory")]
        public IActionResult DeleteAllIdentitiesMemory()
        {
            _identityDAO.DeleteAll();
            return Ok();
        }

        [HttpDelete]
        [Route("api/v1/identity/all/db")]
        public async Task<IActionResult> DeleteAllIdentitiesDbAsync()
        {
            await _identityAsyncDAO.DeleteAllAsync().ConfigureAwait(false);
            return Ok();
        }
    }
}

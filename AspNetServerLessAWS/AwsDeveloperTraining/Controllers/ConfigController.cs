using AwsDeveloperTraining.Config;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AwsDeveloperTraining.Controllers
{
	[Route("api/[controller]")]
	public class ConfigController : Controller
	{
		private readonly IAWSDeveloperTrainingConfigSection _config;

		public ConfigController(IAWSDeveloperTrainingConfigSection config)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
		}

		[HttpGet("Secret")]
		public IActionResult GetEnvironmentSecret()
		{
			return Json(_config.EnvironmentSecret);
		}
	}
}

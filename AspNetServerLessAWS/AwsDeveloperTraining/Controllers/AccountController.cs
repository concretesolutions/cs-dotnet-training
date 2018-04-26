namespace AwsDeveloperTraining.Controllers
{
	using AwsDeveloperTraining.Config;
	using AwsDeveloperTraining.ServiceModel;
	using Microsoft.AspNetCore.Mvc;
	using System;
	using System.Security.Cryptography;
	using System.Text;

	[Route("api/[controller]")]
	public class AccountController : Controller
	{
		private readonly IAWSDeveloperTrainingConfigSection _config;

		public AccountController(IAWSDeveloperTrainingConfigSection config)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
		}

		[HttpPost("")]
		public IActionResult PostAccount([FromBody]PostAccountRequestDTO request)
		{
			var responseDTO = new PostAccountResponseDTO();

			if (request == null)
			{
				responseDTO.StatusCode = 400;
				Response.StatusCode = responseDTO.StatusCode;

				responseDTO.Messages.Add("invalid request object.");
				return Json(responseDTO);
			}

			if (string.IsNullOrWhiteSpace(request.AccountName) || request.AccountName.Length < 5)
			{
				responseDTO.Messages.Add("invalid Account Name.");
			}

			if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
			{
				responseDTO.Messages.Add("invalid Password");
			}

			if (responseDTO.Messages.Count > 0)
			{
				responseDTO.StatusCode = 400;
				Response.StatusCode = responseDTO.StatusCode;

				return Json(responseDTO);
			}

			var passBytes = Encoding.UTF8.GetBytes(request.Password);

			using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.CredentialSalt)))
			{
				var securedPass = hmac.ComputeHash(passBytes);
			}

			responseDTO.StatusCode = 201;
			responseDTO.AccountId = Guid.NewGuid().ToString("N");

			Response.StatusCode = responseDTO.StatusCode;
			return Json(responseDTO);
		}
	}
}

using System.Collections.Generic;

namespace AwsDeveloperTraining.ServiceModel
{
	public class PostAccountResponseDTO
    {
		public int StatusCode { get; set; }

		public string AccountId { get; set; }

		public List<string> Messages { get; private set; }

		public PostAccountResponseDTO()
		{
			Messages = new List<string>();
		}
	}
}

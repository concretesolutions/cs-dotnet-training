namespace AwsDeveloperTraining.Controllers
{
	using System;
	using System.IO;
	using System.Net;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading.Tasks;
	using Amazon.S3;
	using Amazon.S3.Model;
	using AWSPractices;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Mvc;
	using Newtonsoft.Json;

	[Route("api/[controller]")]
	public class S3Controller : Controller
	{
		private const string TrainingBucketName = "aws-developer-training";
		private const string TrainingS3BucketName = "s3-training";
		private const string TrainingFileName = "create-file-exercise.bundle.css";

		private IHostingEnvironment _env;
		private IAmazonS3 _s3Client;

		public S3Controller(IAmazonS3 s3Client, IHostingEnvironment env)
		{
			_s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
			_env = env ?? throw new ArgumentNullException(nameof(env));
		}

		[HttpPost("Bucket")]
		public async Task<IActionResult> CreateBucketAsync()
		{
			var bucketName = $"{TrainingBucketName}/{TrainingS3BucketName}/";
			var sb = new StringBuilder();

			sb.AppendLine($"checking if bucket already exists: {bucketName}");
			var exists = await _s3Client.DoesS3BucketExistAsync(bucketName).ConfigureAwait(false);

			if (exists)
			{
				sb.AppendLine($"bucket already exists: {bucketName}");
				Response.StatusCode = 409;
				return Content(sb.ToString());
			}

			sb.AppendLine($"creating bucket: {bucketName}");
			var awsResponse = await _s3Client.ExecuteWithNoExceptionsAsync(c => { return c.PutBucketAsync(bucketName); }).ConfigureAwait(false);

			sb.AppendLine($"aws response:");
			sb.AppendLine(JsonConvert.SerializeObject(awsResponse, Formatting.Indented));

			Response.StatusCode = (int)awsResponse.HttpStatusCode;
			return Content(sb.ToString());
		}

		[HttpDelete("Bucket")]
		public async Task<IActionResult> DeleteBucketAsync()
		{
			var bucketName = $"{TrainingBucketName}/{TrainingS3BucketName}/";
			var sb = new StringBuilder();

			sb.AppendLine($"checking if bucket {bucketName} is empty...");

			var listRequest = new ListObjectsV2Request()
			{
				BucketName = TrainingBucketName,
				Prefix = $"{TrainingS3BucketName}/",
				MaxKeys = 10
			};

			var listResponse = await _s3Client.ExecuteWithNoExceptionsAsync(c => { return c.ListObjectsV2Async(listRequest); }).ConfigureAwait(false);

			if (listResponse.HttpStatusCode.Equals(HttpStatusCode.OK) == false)
			{
				sb.AppendLine("aws returned an error response:");
				sb.AppendLine(JsonConvert.SerializeObject(listResponse, Formatting.Indented));

				Response.StatusCode = (int)listResponse.HttpStatusCode;
				return Content(sb.ToString());
			}

			if (listResponse.S3Objects.Exists(o => listRequest.Prefix.Equals(o.Key) == false))
			{
				sb.AppendLine("blocked because bucket is not empty.");
				sb.AppendLine("top 10 bucket objects:");

				sb.AppendLine(JsonConvert.SerializeObject(listResponse.S3Objects));
				Response.StatusCode = 403;

				return Content(sb.ToString());
			}

			sb.AppendLine("requesting bucket deletion to s3...");
			var deleteResponse = await _s3Client.ExecuteWithNoExceptionsAsync(c => { return c.DeleteBucketAsync(bucketName); }).ConfigureAwait(false);

			if (deleteResponse.HttpStatusCode.Equals(HttpStatusCode.NoContent))
			{
				sb.AppendLine($"bucket {bucketName} deleted.");
				Response.StatusCode = 200;
			}
			else
			{
				Response.StatusCode = (int)deleteResponse.HttpStatusCode;
			}

			sb.AppendLine("aws response:");
			sb.AppendLine(JsonConvert.SerializeObject(deleteResponse, Formatting.Indented));

			return Content(sb.ToString());
		}

		[HttpPost("File")]
		public async Task<IActionResult> CreateFileAsync()
		{
			var sb = new StringBuilder();
			sb.AppendLine($"checking existence of file {TrainingFileName} in server");

			var filePath = $"{_env.ContentRootPath}/SampleData/S3/{TrainingFileName}";
			var exists = System.IO.File.Exists(filePath);

			if (exists == false)
			{
				sb.AppendLine($"file {TrainingFileName} could not be found.");
				Response.StatusCode = 500;

				return Content(sb.ToString());
			}

			sb.AppendLine($"checking existence of object in s3 bucket.");

			var objectKey = $"{TrainingS3BucketName}/{TrainingFileName}";

			var metadataResponse = await _s3Client
				.ExecuteWithNoExceptionsAsync(c => { return c.GetObjectMetadataAsync(TrainingBucketName, objectKey); })
				.ConfigureAwait(false);

			if (metadataResponse.HttpStatusCode.Equals(HttpStatusCode.OK))
			{
				sb.AppendLine("file already exists.");
				Response.StatusCode = 409;
				return Content(sb.ToString());
			}

			if (metadataResponse.HttpStatusCode.Equals(HttpStatusCode.NotFound) == false)
			{
				sb.AppendLine("aws returned an error response:");
				sb.AppendLine(JsonConvert.SerializeObject(metadataResponse, Formatting.Indented));

				Response.StatusCode = (int)metadataResponse.HttpStatusCode;
				return Content(sb.ToString());
			}

			sb.AppendLine("sending file to s3.");
			var putObjectRequest = new PutObjectRequest();

			using (var alg = MD5.Create())
			{
				var contentEncoding = Encoding.UTF8;
				var fileContent = await System.IO.File.ReadAllTextAsync(filePath, contentEncoding).ConfigureAwait(false);

				var contentMD5 = Convert.ToBase64String(alg.ComputeHash(contentEncoding.GetBytes(fileContent)));
				putObjectRequest.MD5Digest = contentMD5;

				putObjectRequest.BucketName = TrainingBucketName;
				putObjectRequest.Key = objectKey;

				putObjectRequest.ContentType = "text/css";
				putObjectRequest.FilePath = filePath;
			}

			var putObjectResponse = await _s3Client
				.ExecuteWithNoExceptionsAsync(c => { return c.PutObjectAsync(putObjectRequest); })
				.ConfigureAwait(false);

			sb.AppendLine
			(
				putObjectResponse.HttpStatusCode.Equals(HttpStatusCode.OK) ?
				"file created with success:" :
				"aws returned an error response:"
			);

			sb.AppendLine(JsonConvert.SerializeObject(putObjectResponse, Formatting.Indented));

			Response.StatusCode = (int)putObjectResponse.HttpStatusCode;
			return Content(sb.ToString());
		}

		[HttpDelete("File")]
		public async Task<IActionResult> DeleteFileAsync()
		{
			var objectKey = $"{TrainingS3BucketName}/{TrainingFileName}";
			var sb = new StringBuilder();

			sb.AppendLine("requesting deletion to s3..");

			var awsResponse = await _s3Client
				.ExecuteWithNoExceptionsAsync(c => { return c.DeleteObjectAsync(TrainingBucketName, objectKey); })
				.ConfigureAwait(false);

			if (awsResponse.HttpStatusCode.Equals(HttpStatusCode.NoContent))
			{
				sb.AppendLine($"object {TrainingBucketName}/{objectKey} deleted");
				Response.StatusCode = 200;
			}
			else
			{
				Response.StatusCode = (int)awsResponse.HttpStatusCode;
			}

			sb.AppendLine("aws response:");
			sb.AppendLine(JsonConvert.SerializeObject(awsResponse, Formatting.Indented));
			return Content(sb.ToString());
		}

		[HttpGet("File/" + TrainingFileName)]
		public async Task<IActionResult> GetFileAsync()
		{
			var objectKey = $"{TrainingS3BucketName}/{TrainingFileName}";

			var awsResponse = await _s3Client
				.ExecuteWithNoExceptionsAsync(c => { return c.GetObjectAsync(TrainingBucketName, objectKey); })
				.ConfigureAwait(false);

			var content = "";

			if (awsResponse.HttpStatusCode.Equals(HttpStatusCode.OK))
			{
				using (var reader = new StreamReader(awsResponse.ResponseStream, Encoding.UTF8))
				{
					content = await reader.ReadToEndAsync().ConfigureAwait(false);
				}
			}

			Response.ContentType = "text/css";
			Response.StatusCode = (int)awsResponse.HttpStatusCode;

			return Content(content);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			_s3Client.Dispose();
			_s3Client = null;
		}
	}
}

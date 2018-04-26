namespace AWSDeveloperTraining.Practices
{
	using Amazon.Lambda;
	using Amazon.S3;

	public interface IAWSClientFactory
    {
		IAmazonS3 CreateS3Client();

		IAmazonLambda CreateLambdaClient();
	}
}

using Amazon.S3;
using Amazon.S3.Model;

namespace ImageProcessingService.Services
{
	public class S3Service
	{
		private readonly IAmazonS3 _s3Client;
		private readonly string _bucketName;

		public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
		{
			_s3Client = s3Client;
			_bucketName = configuration["AWS:BucketName"];
		}

		public async Task<String> UploadImageAsync(Stream imageStream, string imageName)
		{
			var putRequest = new PutObjectRequest()
			{
				BucketName = _bucketName,
				Key = imageName,
				InputStream = imageStream,
				ContentType = "image/jpeg"
			};

			var response = await _s3Client.PutObjectAsync(putRequest);
			if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
			{
				throw new Exception("Failed to upload image to S3");
			}
			return $"https://{_bucketName}.s3.amazonaws.com/{imageName}";
		}
		public async Task<Stream> LoadImageAsync(string imageId)
		{
			var imageUrl = $"https://{_bucketName}.s3.amazonaws.com/{imageId}.jpg";
			var request = new GetObjectRequest()
			{
				BucketName = _bucketName,
				Key = $"{imageId}.jpg"
			};

			using var response = await _s3Client.GetObjectAsync(request);
			var memoryStream = new MemoryStream();
			await response.ResponseStream.CopyToAsync(memoryStream);
			memoryStream.Position = 0;
			return memoryStream;
		}
	}
}

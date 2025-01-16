using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Net.Http.Headers;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessingService.Services
{
	public class ImageService
	{
		private readonly S3Service _s3Service;
		private readonly IDistributedCache _cache;

		public ImageService(S3Service s3Service, IDistributedCache cache)
		{
			_s3Service=s3Service;
			_cache=cache;
		}
		private async Task<byte[]> GetCachedImageAsync(string cacheKey)
		{
			return await _cache.GetAsync(cacheKey);
		}
		private async Task SetCachedImageAsync(string cacheKey, byte[] imageData)
		{
			var options = new DistributedCacheEntryOptions()
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
			};
			await _cache.SetAsync(cacheKey, imageData, options);
		}
		public async Task<byte[]> ResizeImageAsync(string imageId, int width, int height)
		{
			var cacheKey = $"resized-{imageId}-{width}-{height}";
			var cachedImage = await GetCachedImageAsync(cacheKey);
			if (cachedImage != null)
			{
				return cachedImage;
			}

			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);
			image.Mutate(x => x.Resize(new ResizeOptions
			{
				Size = new Size(width, height),
				Mode = ResizeMode.Crop
			}));

			
			using var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 75 });
			var imageData = outputStream.ToArray();

			await SetCachedImageAsync(cacheKey, imageData);
			return imageData;
		}

		public async Task<byte[]> CropImageAsync(string imageId, int x, int y, int width, int height)
		{
			//Cache logic. We dont call to AWS S3 if we have the image in cache
			var cacheKey = $"cropped-{imageId}-{x}-{y}-{width}-{height}";
			var cachedImage = await GetCachedImageAsync(cacheKey);
			if (cachedImage != null)
			{
				return cachedImage;
			}

			//Since we dont have the cache we then call the S3 Service
			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);
			image.Mutate(m => m.Crop(new Rectangle(x, y, width, height)));

			using var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 75 });
			var imageData = outputStream.ToArray();

			await SetCachedImageAsync(cacheKey, imageData);
			return imageData;
		}

		public async Task<byte[]> RotateImageAsync(string imageId, float degrees)
		{
			var cacheKey = $"rotate-{imageId}-{degrees}";
			var cachedImage = await GetCachedImageAsync(cacheKey);
			if (cachedImage != null)
			{
				return cachedImage;
			}

			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);
			image.Mutate(x => x.Rotate(degrees));

			using var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 75 });
			var imageData = outputStream.ToArray();

			await SetCachedImageAsync(cacheKey, imageData);
			return imageData;
		}

		public async Task<byte[]> AddWatermarkAsync(string imageId, string watermakText)
		{
			var cacheKey = $"watermark-{imageId}-{watermakText}";
			var cachedImage = await GetCachedImageAsync(cacheKey);
			if (cachedImage != null)
			{
				return cachedImage;
			}

			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);
			var font = SystemFonts.CreateFont("Arial", 20);
			image.Mutate(x => x.DrawText(watermakText, font, Color.White, new PointF(10, 10)));

			using var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 75 });
			var imageData = outputStream.ToArray();

			await SetCachedImageAsync(cacheKey, imageData);
			return imageData;
		}

		public async Task<byte[]> FlipImageAsync(string imageId, FlipMode flipMode)
		{
			var cacheKey = $"flip-{imageId}-{flipMode}";
			var cachedImage = await GetCachedImageAsync(cacheKey);
			if (cachedImage != null)
			{
				return cachedImage;
			}

			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);
			image.Mutate(x => x.Flip(flipMode));

			using var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 75 });
			var imageData = outputStream.ToArray();

			await SetCachedImageAsync(cacheKey, imageData);
			return imageData;
		}

		public async Task<byte[]> MirrorImageAsync(string imageId)
		{
			var cacheKey = $"mirror-{imageId}";
			var cachedImage = await GetCachedImageAsync(cacheKey);
			if (cachedImage != null)
			{
				return cachedImage;
			}

			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);
			image.Mutate(x => x.Flip(FlipMode.Horizontal));

			using var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 75 });
			var imageData = outputStream.ToArray();

			await SetCachedImageAsync(cacheKey, imageData);
			return imageData;
		}

		public async Task<byte[]> CompressImageAsync(string imageId, int quality)
		{
			var cacheKey = $"compress-{imageId}-{quality}";
			var cachedImage = await GetCachedImageAsync(cacheKey);
			if (cachedImage != null)
			{
				return cachedImage;
			}

			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);

			using var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = quality });
			var imageData = outputStream.ToArray();

			await SetCachedImageAsync(cacheKey, imageData);
			return imageData;
		}

		public async Task<byte[]> ChangeFormatAsync(string imageId, string format)
		{
			var cacheKey = $"changeformat-{imageId}-{format}";
			var cachedImage = await GetCachedImageAsync(cacheKey);
			if (cachedImage != null)
			{
				return cachedImage;
			}

			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);

			using var outputStream = new MemoryStream();
			switch (format.ToLower())
			{
				case "png":
					await image.SaveAsPngAsync(outputStream);
					break;
				case "bmp":
					await image.SaveAsBmpAsync(outputStream);
					break;
				case "gif":
					await image.SaveAsGifAsync(outputStream);
					break;
				default:
					await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 75 });
					break;
			}
			var imageData = outputStream.ToArray();

			await SetCachedImageAsync(cacheKey, imageData);
			return imageData;
		}

		public async Task<byte[]> ApplyFilterAsync(string imageId, string filter)
		{
			var cacheKey = $"filter-{imageId}-{filter}";
			var cachedImage = await GetCachedImageAsync(cacheKey);
			if (cachedImage != null)
			{
				return cachedImage;
			}

			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);

			switch (filter.ToLower())
			{
				case "grayscale":
					image.Mutate(x => x.Grayscale());
					break;
				case "sepia":
					image.Mutate(x => x.Sepia());
					break;
				default:
					break;
			}

			using var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 75 });
			var imageData = outputStream.ToArray();

			await SetCachedImageAsync(cacheKey, imageData);
			return imageData;
		}
	}
}

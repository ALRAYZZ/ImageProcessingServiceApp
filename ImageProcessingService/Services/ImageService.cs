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

		public ImageService(S3Service s3Service)
		{
			_s3Service=s3Service;
		}

		public async Task<byte[]> ResizeImageAsync(string imageId, int width, int height)
		{
			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);
			image.Mutate(x => x.Resize(new ResizeOptions
			{
				Size = new Size(width, height),
				Mode = ResizeMode.Crop
			}));

			
			using var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 75 });
			return outputStream.ToArray();
		}

		public async Task<byte[]> CropImageAsync(string imageId, int x, int y, int width, int height)
		{
			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);
			image.Mutate(m => m.Crop(new Rectangle(x, y, width, height)));

			using var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 75 });
			return outputStream.ToArray();
		}

		public async Task<byte[]> RotateImageAsync(string imageId, float degrees)
		{
			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);
			image.Mutate(x => x.Rotate(degrees));

			using var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 75 });
			return outputStream.ToArray();
		}

		public async Task<byte[]> AddWatermarkAsync(string imageId, string watermakText)
		{
			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);
			var font = SystemFonts.CreateFont("Arial", 20);
			image.Mutate(x => x.DrawText(watermakText, font, Color.White, new PointF(10, 10)));

			using var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 75 });
			return outputStream.ToArray();
		}

		public async Task<byte[]> FlipImageAsync(string imageId, FlipMode flipMode)
		{
			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);
			image.Mutate(x => x.Flip(flipMode));

			using var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 75 });
			return outputStream.ToArray();
		}

		public async Task<byte[]> MirrorImageAsync(string imageId)
		{
			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);
			image.Mutate(x => x.Flip(FlipMode.Horizontal));

			using var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 75 });
			return outputStream.ToArray();
		}

		public async Task<byte[]> CompressImageAsync(string imageId, int quality)
		{
			using var imageStream = await _s3Service.LoadImageAsync(imageId);
			using var image = await Image.LoadAsync(imageStream);

			using var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = quality });
			return outputStream.ToArray();
		}

		public async Task<byte[]> ChangeFormatAsync(string imageId, string format)
		{
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
			return outputStream.ToArray();
		}

		public async Task<byte[]> ApplyFilterAsync(string imageId, string filter)
		{
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
			return outputStream.ToArray();
		}
	}
}

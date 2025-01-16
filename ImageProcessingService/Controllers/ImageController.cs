using Amazon.S3.Model;
using ImageProcessingService.Services;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessingService.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ImageController : Controller
	{
		private readonly ImageService _imageService;
		private readonly S3Service _s3Service;

		public ImageController(ImageService imageService, S3Service s3Service)
		{
			_imageService=imageService;
			_s3Service = s3Service;
		}

		[HttpPost("upload")]
		public async Task<IActionResult> UploadImage(IFormFile image)
		{
			if (image == null || image.Length == 0)
			{
				return BadRequest("Image file is required");
			}

			var imageId = Guid.NewGuid().ToString();
			var imageName = $"{imageId}.jpg";

			using var imageStream = image.OpenReadStream();
			var imageUrl = await _s3Service.UploadImageAsync(imageStream, imageName);

			return Ok(new {imageId, imageUrl });
		}

		[HttpPost("resize")]
		public async Task<IActionResult> ResizeImage(string imageId, int width, int height)
		{
			var resizedImage = await _imageService.ResizeImageAsync(imageId, width, height);
			return File(resizedImage, "image/jpeg");
		}
		[HttpPost("crop")]
		public async Task<IActionResult> CropImage(string imageId, int x, int y, int width, int height)
		{
			var croppedImage = await _imageService.CropImageAsync(imageId, x, y, width, height);
			return File(croppedImage, "image/jpeg");
		}
		[HttpPost("rotate")]
		public async Task<IActionResult> RotateImage(string imageId, float degrees)
		{
			var rotatedImage = await _imageService.RotateImageAsync(imageId, degrees);
			return File(rotatedImage, "image/jpeg");
		}
		[HttpPost("watermark")]
		public async Task<IActionResult> AddWatermark(string imageId, string watermarkText)
		{
			var watermarkedImage = await _imageService.AddWatermarkAsync(imageId, watermarkText);
			return File(watermarkedImage, "image/jpeg");
		}
		[HttpPost("flip")]
		public async Task<IActionResult> FlipImage(string imageId, FlipMode flipMode)
		{
			var flippedImage = await _imageService.FlipImageAsync(imageId, flipMode);
			return File(flippedImage, "image/jpeg");
		}
		[HttpPost("mirror")]
		public async Task<IActionResult> MirrorImage(string imageId)
		{
			var mirroredImage = await _imageService.MirrorImageAsync(imageId);
			return File(mirroredImage, "image/jpeg");
		}
		[HttpPost("compress")]
		public async Task<IActionResult> CompressImage(string imageId, int quality)
		{
			var compressedImage = await _imageService.CompressImageAsync(imageId, quality);
			return File(compressedImage, "image/jpeg");
		}
		[HttpPost("changeformat")]
		public async Task<IActionResult> ChangeFormat(string imageId, string format)
		{
			var formattedImage = await _imageService.ChangeFormatAsync(imageId, format);
			return File(formattedImage, $"image/{format}");
		}
		[HttpPost("applyfilter")]
		public async Task<IActionResult> ApplyFilter(string imageId, string filter)
		{
			var filteredImage = await _imageService.ApplyFilterAsync(imageId, filter);
			return File(filteredImage, "image/jpeg");
		}
	}
}

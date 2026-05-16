namespace GLMS.Web_POE.Services
{
	public class FileService : IFileService
	{
		private readonly IWebHostEnvironment _environment;
		private readonly string _uploadFolder;

		public FileService(IWebHostEnvironment environment)
		{
			_environment = environment;
			_uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "agreements");

			if (!Directory.Exists(_uploadFolder))
			{
				Directory.CreateDirectory(_uploadFolder);
			}
		}

		public bool IsPdf(IFormFile file)
		{
			if (file == null || file.Length == 0)
				return false;

			var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
			return extension == ".pdf" && file.ContentType == "application/pdf";
		}

		public async Task<(string fileName, string filePath)> SaveAgreementAsync(IFormFile file)
		{
			if (!IsPdf(file))
				throw new InvalidOperationException("Only PDF files are allowed.");

			var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
			var fullPath = Path.Combine(_uploadFolder, uniqueFileName);

			using var stream = new FileStream(fullPath, FileMode.Create);
			await file.CopyToAsync(stream);

			var relativePath = Path.Combine("uploads", "agreements", uniqueFileName).Replace("\\", "/");

			return (uniqueFileName, relativePath);
		}
	}
}

namespace GLMS.Web_POE.Services
{
	public interface IFileService
	{
		bool IsPdf(IFormFile file);
		Task<(string fileName, string filePath)> SaveAgreementAsync(IFormFile file);
	}
}
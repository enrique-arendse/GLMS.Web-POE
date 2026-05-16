using GLMS.Web_POE.Models;

namespace GLMS.Web_POE.Services
{
	public interface IContractService
	{
		bool CanCreateServiceRequest(Contract contract);
	}
}

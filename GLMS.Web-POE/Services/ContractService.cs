using GLMS.Web_POE.Models;

namespace GLMS.Web_POE.Services
{
	public class ContractService : IContractService
	{
		public bool CanCreateServiceRequest(Contract contract)
		{
			if (contract == null)
				return false;

			return contract.Status != ContractStatus.Expired &&
				   contract.Status != ContractStatus.OnHold;
		}
	}
}
using Leasing.Data.Entities;
using Leasing.Models;
using System.Threading.Tasks;

namespace Leasing.Helpers
{
    public interface IConverterHelper
    {
        Task<Contract> ToContractAsync(ContractViewModel model, bool isNew);

        ContractViewModel ToContractViewModel(Contract contract);

    }
}

using Leasing.Data;
using Leasing.Data.Entities;
using Leasing.Models;
using System.Threading.Tasks;

namespace Leasing.Helpers
{

    public class ConverterHelper : IConverterHelper
    {

        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        public ConverterHelper(DataContext context, ICombosHelper combosHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
        }

        public async Task<Contract> ToContractAsync(ContractViewModel model, bool isNew)
        {
            return new Contract
            {
                EndDate = model.EndDate,
                Id = isNew ? 0 : model.Id,
                IsActive = model.IsActive,
                Lessee = await _context.Lessees.FindAsync(model.LesseeId),
                Owner = await _context.Owners.FindAsync(model.OwnerId),
                Price = model.Price,
                Property = await _context.Properties.FindAsync(model.PropertyId),
                Remarks = model.Remarks,
                StartDate = model.StartDate
            };
        }

        public ContractViewModel ToContractViewModel(Contract contract)
        {
            return new ContractViewModel
            {
                EndDate = contract.EndDate,
                IsActive = contract.IsActive,
                LesseeId = contract.Lessee.Id,
                OwnerId = contract.Owner.Id,
                Price = contract.Price,
                Remarks = contract.Remarks,
                StartDate = contract.StartDate,
                Id = contract.Id,
                Lessees = _combosHelper.GetComboLessees(),
                PropertyId = contract.Property.Id
            };
        }

    }
}

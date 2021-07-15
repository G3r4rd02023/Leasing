using Leasing.Data;
using Leasing.Data.Entities;
using Leasing.Helpers;
using Leasing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leasing.Controllers
{
    [Authorize(Roles = "Manager")]
    public class OwnersController : Controller
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IConverterHelper _converterHelper;

        public OwnersController(DataContext context,IUserHelper userHelper,
            IImageHelper imageHelper,IConverterHelper converterHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _imageHelper = imageHelper;
            _converterHelper = converterHelper;
        }

        // GET: Owners
        public IActionResult Index()
        {
            return View(_context.Owners
                .Include(o => o.User)
                .Include(o => o.Properties)
                .Include(o => o.Contracts));
        }


        // GET: Owners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _context.Owners
            .Include(o => o.User)
            .Include(o => o.Properties)
            .ThenInclude(p => p.PropertyType)
            .Include(o => o.Properties)
            .ThenInclude(p => p.PropertyImages)
            .Include(o => o.Contracts)
            .FirstOrDefaultAsync(m => m.Id == id);

            if (owner == null)
            {
                return NotFound();
            }

            return View(owner);
        }

        // GET: Owners/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Owners/Create       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddUserViewModel view)
        {
            if (ModelState.IsValid)
            {
                var user = await AddUser(view);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Este email ya esta en uso.");
                    return View(view);
                }

                var owner = new Owner
                {
                    Properties = new List<Property>(),
                    Contracts = new List<Contract>(),
                    User = user,
                };

                _context.Owners.Add(owner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(view);
        }

        private async Task<User> AddUser(AddUserViewModel view)
        {
            var user = new User
            {
                Address = view.Address,
                Document = view.Document,
                Email = view.Username,
                FirstName = view.FirstName,
                LastName = view.LastName,
                PhoneNumber = view.PhoneNumber,
                UserName = view.Username
            };

            var result = await _userHelper.AddUserAsync(user, view.Password);
            if (result != IdentityResult.Success)
            {
                return null;
            }

            var newUser = await _userHelper.GetUserByEmailAsync(view.Username);
            await _userHelper.AddUserToRoleAsync(newUser, "Owner");
            return newUser;
        }


        // GET: Owners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _context.Owners
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id.Value);
            if (owner == null)
            {
                return NotFound();
            }

            var view = new EditUserViewModel
            {
                Address = owner.User.Address,
                Document = owner.User.Document,
                FirstName = owner.User.FirstName,
                Id = owner.Id,
                LastName = owner.User.LastName,
                PhoneNumber = owner.User.PhoneNumber
            };

            return View(view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel view)
        {
            if (ModelState.IsValid)
            {
                var owner = await _context.Owners
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == view.Id);

                owner.User.Document = view.Document;
                owner.User.FirstName = view.FirstName;
                owner.User.LastName = view.LastName;
                owner.User.Address = view.Address;
                owner.User.PhoneNumber = view.PhoneNumber;

                await _userHelper.UpdateUserAsync(owner.User);
                return RedirectToAction(nameof(Index));
            }

            return View(view);
        }



        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _context.Owners
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (owner == null)
            {
                return NotFound();
            }

            _context.Owners.Remove(owner);
            await _context.SaveChangesAsync();
            await _userHelper.DeleteUserAsync(owner.User.Email);
            return RedirectToAction(nameof(Index));
        }


        private bool OwnerExists(int id)
        {
            return _context.Owners.Any(e => e.Id == id);
        }

        public async Task<IActionResult> AddProperty(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _context.Owners.FindAsync(id.Value);
            if (owner == null)
            {
                return NotFound();
            }

            var view = new PropertyViewModel
            {
                OwnerId = owner.Id,
                PropertyTypes = GetComboPropertyTypes()
            };

            return View(view);
        }

        [HttpPost]
        public async Task<IActionResult> AddProperty(PropertyViewModel view)
        {
            if (ModelState.IsValid)
            {
                var property = await ToPropertyAsync(view);
                _context.Properties.Add(property);
                await _context.SaveChangesAsync();
                //return RedirectToAction($"{nameof(Details)}/{view.OwnerId}");
                return RedirectToAction("Details", new { id = view.OwnerId });
            }

            return View(view);
        }

        private async Task<Property> ToPropertyAsync(PropertyViewModel view)
        {
            return new Property
            {
                Address = view.Address,
                HasParkingLot = view.HasParkingLot,
                IsAvailable = view.IsAvailable,
                Neighborhood = view.Neighborhood,
                Price = view.Price,
                Rooms = view.Rooms,
                SquareMeters = view.SquareMeters,
                Stratum = view.Stratum,
                Owner = await _context.Owners.FindAsync(view.OwnerId),
                PropertyType = await _context.PropertyTypes.FindAsync(view.PropertyTypeId),
                Remarks = view.Remarks
            };
        }

        private IEnumerable<SelectListItem> GetComboPropertyTypes()
        {
            var list = _context.PropertyTypes.Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id.ToString()
            }).OrderBy(p => p.Text).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "(Seleccione un tipo de propiedad...)",
                Value = "0"
            });

            return list;
        }

        public async Task<IActionResult> EditProperty(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .Include(p => p.Owner)
                .Include(p => p.PropertyType)
                .FirstOrDefaultAsync(p => p.Id == id.Value);
            if (property == null)
            {
                return NotFound();
            }

            var view = ToPropertyViewModel(property);
            return View(view);
        }

        private PropertyViewModel ToPropertyViewModel(Property property)
        {
            return new PropertyViewModel
            {
                Address = property.Address,
                HasParkingLot = property.HasParkingLot,
                Id = property.Id,
                IsAvailable = property.IsAvailable,
                Neighborhood = property.Neighborhood,
                Price = property.Price,
                Rooms = property.Rooms,
                SquareMeters = property.SquareMeters,
                Stratum = property.Stratum,
                Owner = property.Owner,
                OwnerId = property.Owner.Id,
                PropertyType = property.PropertyType,
                PropertyTypeId = property.PropertyType.Id,
                PropertyTypes = GetComboPropertyTypes(),
                Remarks = property.Remarks,
            };
        }

        [HttpPost]
        public async Task<IActionResult> EditProperty(PropertyViewModel view)
        {
            if (ModelState.IsValid)
            {
                var property = await ToPropertyAsync(view);
                _context.Properties.Update(property);
                await _context.SaveChangesAsync();
                //return RedirectToAction($"{nameof(Details)}/{view.OwnerId}");
                return RedirectToAction("Details", new { id = view.OwnerId });
            }

            return View(view);
        }

        public async Task<IActionResult> DetailsProperty(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .Include(o => o.Owner)
                .ThenInclude(o => o.User)
                .Include(o => o.Contracts)
                .ThenInclude(c => c.Lessee)
                .ThenInclude(l => l.User)
                .Include(o => o.PropertyType)
                .Include(p => p.PropertyImages)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        public async Task<IActionResult> AddImage(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties.FindAsync(id.Value);
            if (property == null)
            {
                return NotFound();
            }

            var model = new PropertyImageViewModel
            {
                Id = property.Id
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddImage(PropertyImageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var path = string.Empty;

                if (model.ImageFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(model.ImageFile);
                }

                var propertyImage = new PropertyImage
                {
                    ImageUrl = path,
                    Property = await _context.Properties.FindAsync(model.Id)
                };

                _context.PropertyImages.Add(propertyImage);
                await _context.SaveChangesAsync();
                //return RedirectToAction($"{nameof(DetailsProperty)}/{model.Id}");
                return RedirectToAction("DetailsProperty", new { id = model.Id });
            }

            return View(model);
        }

        public async Task<IActionResult> AddContract(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == id.Value);
            if (property == null)
            {
                return NotFound();
            }

            var view = new ContractViewModel
            {
                OwnerId = property.Owner.Id,
                PropertyId = property.Id,
                Lessees = GetComboLessees(),
                Price = property.Price,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            };

            return View(view);
        }

        private IEnumerable<SelectListItem> GetComboLessees()
        {
            var list = _context.Lessees.Include(l => l.User).Select(p => new SelectListItem
            {
                Text = p.User.FullNameWithDocument,
                Value = p.Id.ToString()
            }).OrderBy(p => p.Text).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "(Select a lessee...)",
                Value = "0"
            });

            return list;
        }

        [HttpPost]
        public async Task<IActionResult> AddContract(ContractViewModel view)
        {
            if (ModelState.IsValid)
            {
                var contract = await ToContractAsync(view);
                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();
                //return RedirectToAction($"{nameof(DetailsProperty)}/{view.OwnerId}");
                return RedirectToAction("DetailsProperty", new { id = view.OwnerId});
            }

            return View(view);
        }

        private async Task<Contract> ToContractAsync(ContractViewModel view)
        {
            return new Contract
            {
                EndDate = view.EndDate,
                IsActive = view.IsActive,
                Lessee = await _context.Lessees.FindAsync(view.LesseeId),
                Owner = await _context.Owners.FindAsync(view.OwnerId),
                Price = view.Price,
                Property = await _context.Properties.FindAsync(view.PropertyId),
                Remarks = view.Remarks,
                StartDate = view.StartDate,
                Id = view.Id
            };
        }

        public async Task<IActionResult> EditContract(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(p => p.Owner)
                .Include(p => p.Lessee)
                .Include(p => p.Property)
                .FirstOrDefaultAsync(p => p.Id == id.Value);
            if (contract == null)
            {
                return NotFound();
            }

            return View(_converterHelper.ToContractViewModel(contract));
        }

        [HttpPost]
        public async Task<IActionResult> EditContract(ContractViewModel view)
        {
            if (ModelState.IsValid)
            {
                var contract = await _converterHelper.ToContractAsync(view, false);
                _context.Contracts.Update(contract);
                await _context.SaveChangesAsync();
                //return RedirectToAction($"{nameof(DetailsProperty)}/{view.OwnerId}");
                return RedirectToAction("DetailsProperty", new { id = view.OwnerId});
            }

            return View(view);
        }

        public async Task<IActionResult> DeleteImage(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propertyImage = await _context.PropertyImages
                .Include(pi => pi.Property)
                .FirstOrDefaultAsync(pi => pi.Id == id.Value);
            if (propertyImage == null)
            {
                return NotFound();
            }

            _context.PropertyImages.Remove(propertyImage);
            await _context.SaveChangesAsync();
            //return RedirectToAction($"{nameof(DetailsProperty)}/{propertyImage.Property.Id}");
            return RedirectToAction("DetailsProperty", new { id = propertyImage.Property.Id });
        }

        public async Task<IActionResult> DeleteContract(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Property)
                .FirstOrDefaultAsync(c => c.Id == id.Value);
            if (contract == null)
            {
                return NotFound();
            }

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            //return RedirectToAction($"{nameof(DetailsProperty)}/{contract.Property.Id}");
            return RedirectToAction("DetailsProperty", new { id = contract.Property.Id });
        }

        public async Task<IActionResult> DeleteProperty(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(pi => pi.Id == id.Value);
            if (property == null)
            {
                return NotFound();
            }

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();
           // return RedirectToAction($"{nameof(Details)}/{property.Owner.Id}");
            return RedirectToAction("Details", new { id = property.Owner.Id });
        }

        public async Task<IActionResult> DetailsContract(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Owner)
                .ThenInclude(o => o.User)
                .Include(c => c.Lessee)
                .ThenInclude(o => o.User)
                .Include(c => c.Property)
                .ThenInclude(p => p.PropertyType)
                .FirstOrDefaultAsync(pi => pi.Id == id.Value);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }


    }
}

using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Leasing.Helpers
{
    public interface ICombosHelper
    {
        IEnumerable<SelectListItem> GetComboLessees();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leasing.Data.Entities
{
    public class Lessee
    {
        public int Id { get; set; }

        public User User { get; set; }

        public ICollection<Contract> Contracts { get; set; }
    }
}

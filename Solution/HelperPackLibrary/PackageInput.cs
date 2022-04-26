using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperPackLibrary
{
    class PackageInput
    {
        public double WeightLimit { get; set; }

        public List<Item> Items { get; set; } = new List<Item>();
    }

}

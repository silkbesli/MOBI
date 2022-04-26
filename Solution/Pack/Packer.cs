using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pack
{
    public static class Packer
    {

        static string Pack(string path)
        {
            return  HelperPackLibrary.Packer.Pack(path);
        }
            
    }
}

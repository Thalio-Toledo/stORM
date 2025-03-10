using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BonesCoreOrm.Generators.Intefaces
{
    public interface IGenerator
    {
        string Generate(dynamic entity = null);
    }
}

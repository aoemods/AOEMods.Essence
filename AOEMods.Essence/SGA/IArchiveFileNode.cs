using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMods.Essence.SGA
{
    public interface IArchiveFileNode : IArchiveNode
    {
        public string Extension => Path.GetExtension(Name);
        public IEnumerable<byte> GetData();
    }
}

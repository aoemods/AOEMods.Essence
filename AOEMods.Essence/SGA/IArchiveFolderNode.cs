using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMods.Essence.SGA;

public interface IArchiveFolderNode : IArchiveNode
{
    IList<IArchiveNode> Children { get; }
}

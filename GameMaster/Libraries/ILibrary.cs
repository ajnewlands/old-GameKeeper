using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameMaster.Libraries
{
    public interface ILibrary
    {
        string GetHomePath();
        List<string> GetGameDirectories();
        List<string> GetReparsePoints();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameKeeper
{
    public interface ILibrary
    {
        string GetHomePath();
        List<string> GetGameDirectories();
        List<string> GetReparsePoints();
    }
}

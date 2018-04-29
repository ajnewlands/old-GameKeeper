using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GameMaster.Libraries
{
    public class SteamLibrary : ILibrary
    {
        private string _path;

        public SteamLibrary( ILibraryLocator loc )
        {
            _path = loc.GetLibraryPath();
        }

        public string GetHomePath()
        {
            return _path;
        }

        public List<string> GetGameDirectories()
        {
            return GetFileSystemEntries(FileAttributes.Directory);
        }

        public List<string> GetReparsePoints()
        {
            return GetFileSystemEntries(FileAttributes.ReparsePoint);
        }

        private List<string> GetFileSystemEntries( FileAttributes attr)
        {
            var items = new List<string>();
            foreach (var directory in System.IO.Directory.GetDirectories(_path))
            {
                if ( (File.GetAttributes(directory) & attr) == attr)
                {
                    items.Add(Path.GetFileName(directory));
                }
                else
                {
                    
                }
            }
            return items;
        }
    }
}

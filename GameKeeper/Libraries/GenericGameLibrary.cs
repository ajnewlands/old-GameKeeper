using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace GameKeeper
{   
/// <summary>
/// A GenericGameLibrary is one where there is a single directory with named subdirectories, one per game.
/// We can infer the content of each subdirectory from the directory name.
/// We can relocate the game simply by moving these directories and linking the old location to the new.
/// </summary>
    public class GenericGameLibrary : ILibrary
    {
        private string _path;

        public GenericGameLibrary( ILibraryLocator loc )
        {
            _path = loc.GetLibraryPath();
        }

        public string GetHomePath()
        {
            return _path;
        }

        public List<string> GetGameDirectories()
        {
            return GetFileSystemEntries(
                FileAttributes.ReparsePoint, false
            );
        }

        public List<string> GetReparsePoints()
        {
            return GetFileSystemEntries(FileAttributes.ReparsePoint, true);
        }

        private List<string> GetFileSystemEntries( FileAttributes attr, bool has_attr)
        {
            var items = new List<string>();
            foreach (var directory in System.IO.Directory.GetDirectories(_path))
            {
                if ( ((File.GetAttributes(directory) & attr) == attr )== has_attr)
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

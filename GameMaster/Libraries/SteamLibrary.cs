using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualBasic.FileIO;

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

        public int MoveGameDirectory( string dir, string new_path )
        {
            FileSystem.MoveDirectory(_path + "\\" + dir, new_path + "\\" + dir, UIOption.AllDialogs, UICancelOption.ThrowException);
            return 0;
        }
    }
}

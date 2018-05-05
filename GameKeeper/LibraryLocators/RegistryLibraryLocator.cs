using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;

namespace GameKeeper
{
    class RegistryLibraryLocator : ILibraryLocator
    {
        private string _path = null;

        public RegistryLibraryLocator( string registryKey, string registryVal, string suffix = null)
        {
            RegistryKey hklm64 = Registry.LocalMachine;

            try
            {
                var path = hklm64.OpenSubKey(registryKey, false).GetValue( registryVal );

                if (path != null)
                {
                    _path = path.ToString();

                    if (suffix != null)
                        _path = Path.Combine(_path, suffix);
                }
            }
            catch (System.IO.IOException)
            {
                // Steam not found.
            }
        }

        public string GetLibraryPath()
        {
            return _path;
        }
    }
}

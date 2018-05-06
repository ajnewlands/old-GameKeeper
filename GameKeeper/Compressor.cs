using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace GameKeeper
{
    public static class Compressor
    {
        private const int FSCTL_SET_COMPRESSION = 0x9C040;
        private const int FSCTL_GET_COMPRESSION = 0X9003C;
        private const short COMPRESSION_FORMAT_NONE = 0;
        private const short COMPRESSION_FORMAT_DEFAULT = 1;
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint OPEN_EXISTING = 3;
        private const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

        [Serializable]
        public class CompressionFailedException : System.Exception
        {
            public CompressionFailedException() : base() { }

            public CompressionFailedException(int Win32ErrorCode) : base(new Win32Exception(Win32ErrorCode).Message) { }

            public CompressionFailedException(string message) : base(message) { }

            public CompressionFailedException(string message, System.Exception inner) : base(message, inner) { }

            protected CompressionFailedException(System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context)
            { }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]  
	        public static extern bool DeviceIoControl(IntPtr hDevice, int
	        dwIoControlCode, ref short lpInBuffer, int nInBufferSize, IntPtr
	        lpOutBuffer, int nOutBufferSize, ref int lpBytesReturned, IntPtr
	        lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess,
            uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        private static void SetCompressionState( string path, short state )
        {
            int lpBytesReturned = 0;

            SafeFileHandle h = CreateFile(path, GENERIC_WRITE|GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero);

            if (h.IsInvalid)
                throw new CreationFailedException(Marshal.GetLastWin32Error());

            var r = DeviceIoControl(h.DangerousGetHandle(), FSCTL_SET_COMPRESSION,
                ref state, 2, IntPtr.Zero, 0,
                ref lpBytesReturned, IntPtr.Zero);

            h.Close();

            if (!r)
                throw new CreationFailedException(Marshal.GetLastWin32Error());

        }

        public static void GetCompressionState( string path, out bool compressed )
        {
            DirectoryInfo d = new DirectoryInfo(path);
            if ( (d.Attributes & FileAttributes.Compressed) == FileAttributes.Compressed)
            {
                compressed = true;
                return;
            }
            compressed = false;
        }

        public static void Compress( string path )
        {
            SetCompressionState(path, COMPRESSION_FORMAT_DEFAULT);
        }

        public static void Decompress( string path )
        {
            SetCompressionState(path, COMPRESSION_FORMAT_NONE);
        }
    }
}

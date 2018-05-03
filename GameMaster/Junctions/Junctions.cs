using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace GameMaster.Junctions
{
    [Serializable]
    public class CreationFailedException : System.Exception
    {
        public CreationFailedException(): base() { }

        public CreationFailedException(int Win32ErrorCode) : base(new Win32Exception(Win32ErrorCode).Message) { }

        public CreationFailedException(string message) : base(message) { }

        public CreationFailedException(string message, System.Exception inner) : base(message, inner) { }

        protected CreationFailedException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }

    [Serializable]
    public class DeletionFailedException : System.Exception
    {
        public DeletionFailedException() : base() { }

        public DeletionFailedException(int Win32ErrorCode) : base(new Win32Exception(Win32ErrorCode).Message) { }

        public DeletionFailedException(string message) : base(message) { }

        public DeletionFailedException(string message, System.Exception inner) : base(message, inner) { }

        protected DeletionFailedException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }

    public class DereferenceFailedException : System.Exception
    {
        public DereferenceFailedException() : base() { }

        public DereferenceFailedException(int Win32ErrorCode) : base(new Win32Exception(Win32ErrorCode).Message) { }

        public DereferenceFailedException(string message) : base(message) { }

        public DereferenceFailedException(string message, System.Exception inner) : base(message, inner) { }

        protected DereferenceFailedException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }

    public static class Junctions
    {
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint GENERIC_READ = 0x80000000;
        private const uint CREATE_NEW = 1;
        private const uint CREATE_ALWAYS = 2;
        private const uint OPEN_EXISTING = 3;
        private const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
        private const int FSCTL_SET_REPARSE_POINT = 0x000900A4;
        private const int FSCTL_GET_REPARSE_POINT = 0x000900A8;
        private const int FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
        private const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

        private const string prefix = @"\??\";

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess,
            uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
            IntPtr InBuffer, int nInBufferSize,
            IntPtr OutBuffer, int nOutBufferSize,
            out int pBytesReturned, IntPtr lpOverlapped
        );

        [StructLayout(LayoutKind.Sequential)]
        private struct REPARSE_DATA_BUFFER
        {
            public uint ReparseTag;
            public ushort ReparseDataLength;
            public ushort Reserved;
            public ushort SubstituteNameOffset;
            public ushort SubstituteNameLength;
            public ushort PrintNameOffset;
            public ushort PrintNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3FF0)] public byte[] PathBuffer;
        }

        public static void CreateJunction( string name, string target )
        {
            // If the target doesn't include a full path, assume relative to the current directory.
            if (!Path.IsPathRooted(target))
                target = Path.Combine(Directory.GetCurrentDirectory(), target);
            // likewise if the link name omits a path, make it relative to the current directory.
            if (!Path.IsPathRooted(name))
                name = Path.Combine(Directory.GetCurrentDirectory(), name);
            // Check target exists before trying to create a link
            if (!Directory.Exists(target))
                throw new CreationFailedException("Target directory " + target + " does not exist");

            // Create a directory to populate with our reparse point
            // If it isn't empty, create file will return 'directory not empty..'
            Directory.CreateDirectory(name);
            
            SafeFileHandle h = CreateFile(name, GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OPEN_REPARSE_POINT, IntPtr.Zero);
            try
            {           
                if (h.IsInvalid)
                    throw new CreationFailedException(Marshal.GetLastWin32Error());

                // Transform the file into a reparse point           
                var targetBytes = Encoding.Unicode.GetBytes(prefix + target);
                var reparseDataBuffer =
                    new REPARSE_DATA_BUFFER
                    {
                        ReparseTag = IO_REPARSE_TAG_MOUNT_POINT,
                        ReparseDataLength = (ushort)(targetBytes.Length + 12),
                        SubstituteNameOffset = 0,
                        SubstituteNameLength = (ushort)targetBytes.Length,
                        PrintNameOffset = (ushort)(targetBytes.Length + 2),
                        PrintNameLength = 0,
                        PathBuffer = new byte[32768] // This should not be fixed
                    };

                Array.Copy(targetBytes, reparseDataBuffer.PathBuffer, targetBytes.Length);

                var inBufferSize = Marshal.SizeOf(reparseDataBuffer);
                var inBuffer = Marshal.AllocHGlobal(inBufferSize);

                Marshal.StructureToPtr(reparseDataBuffer, inBuffer, false);
        
                var r = DeviceIoControl(h.DangerousGetHandle(), FSCTL_SET_REPARSE_POINT,
                    inBuffer, targetBytes.Length + 20, IntPtr.Zero, 0, out int bytesReturned, IntPtr.Zero);

                Marshal.FreeHGlobal(inBuffer);
                if (!r)
                    throw new CreationFailedException(Marshal.GetLastWin32Error());
            }
            finally
            {
                h.Close();
            }
            return;
        }

        public static void DeleteJunction( string name )
        {
            if ((File.GetAttributes(name) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                Directory.Delete(name);
            else // Not a junction
                throw new DeletionFailedException(name + " is not a valid junction");
            return;
        }

        public static void GetJunctionTarget( string name, out string target )
        {
            target = "";
            IntPtr outBuffer;

            if (!Path.IsPathRooted(name))
                name = Path.Combine(Directory.GetCurrentDirectory(), name);

            SafeFileHandle h = CreateFile(name, GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OPEN_REPARSE_POINT, IntPtr.Zero);
            if (h.IsInvalid)
                throw new DereferenceFailedException( Marshal.GetLastWin32Error() );

            try
            {
                var outBufferSize = Marshal.SizeOf(typeof(REPARSE_DATA_BUFFER));
                outBuffer = Marshal.AllocHGlobal(outBufferSize);

                var r = DeviceIoControl(h.DangerousGetHandle(), FSCTL_GET_REPARSE_POINT,
                    IntPtr.Zero, 0, outBuffer, outBufferSize, out int bytesReturned, IntPtr.Zero);

                if (!r)
                    throw new DereferenceFailedException(Marshal.GetLastWin32Error());

                var reparseDataBuffer = (REPARSE_DATA_BUFFER)
                Marshal.PtrToStructure(outBuffer, typeof(REPARSE_DATA_BUFFER));

                if (reparseDataBuffer.ReparseTag != IO_REPARSE_TAG_MOUNT_POINT)
                    throw new DereferenceFailedException(target + " is not a junction");

                var targetDir = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer,
                    reparseDataBuffer.SubstituteNameOffset, reparseDataBuffer.SubstituteNameLength);

                target = targetDir.Substring(prefix.Length);
            }
            finally
            {
                h.Close();
            }

            Marshal.FreeHGlobal(outBuffer);
            return;
        }
    }
}

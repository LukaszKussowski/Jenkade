using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Jenkade.Core;

namespace Jenkade.App;

/// <summary>Keeps secrets as generic credentials of the Windows user, under Control Panel → Credential Manager.</summary>
[SupportedOSPlatform("windows")]
internal sealed partial class WindowsCredentialManager : ICredentialStore
{
    private const int GenericCredential = 1;
    private const int PersistLocalMachine = 2;
    private const int ErrorNotFound = 1168;

    public unsafe void Save(string key, string secret)
    {
        var secretBytes = Encoding.Unicode.GetBytes(secret);
        fixed (char* targetName = key)
        fixed (byte* blob = secretBytes)
        {
            var credential = new Credential
            {
                Type = GenericCredential,
                TargetName = targetName,
                CredentialBlobSize = secretBytes.Length,
                CredentialBlob = blob,
                Persist = PersistLocalMachine,
            };
            if (!CredWrite(&credential, 0))
                throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
    }

    public unsafe string? Read(string key)
    {
        if (!CredRead(key, GenericCredential, 0, out var credential))
        {
            var error = Marshal.GetLastPInvokeError();
            return error == ErrorNotFound ? null : throw new Win32Exception(error);
        }
        try
        {
            return Encoding.Unicode.GetString(credential->CredentialBlob, credential->CredentialBlobSize);
        }
        finally
        {
            CredFree(credential);
        }
    }

    // CREDENTIALW from wincred.h.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct Credential
    {
        public int Flags;
        public int Type;
        public char* TargetName;
        public char* Comment;
        public long LastWritten;
        public int CredentialBlobSize;
        public byte* CredentialBlob;
        public int Persist;
        public int AttributeCount;
        public nint Attributes;
        public char* TargetAlias;
        public char* UserName;
    }

    [LibraryImport("advapi32.dll", EntryPoint = "CredWriteW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool CredWrite(Credential* credential, int flags);

    [LibraryImport("advapi32.dll", EntryPoint = "CredReadW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool CredRead(string targetName, int type, int flags, out Credential* credential);

    [LibraryImport("advapi32.dll")]
    private static unsafe partial void CredFree(void* buffer);
}

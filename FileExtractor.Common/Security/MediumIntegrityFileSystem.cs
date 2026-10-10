using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace FileExtractor.Common.Security;

[SupportedOSPlatform("windows")]
internal sealed class MediumIntegrityFileSystem : IMediumIntegrityFileSystem
{
    // Token access rights: only what duplicating, lowering and impersonating need
    private const uint TokenDuplicate = 0x0002;
    private const uint TokenImpersonate = 0x0004;
    private const uint TokenQuery = 0x0008;
    private const uint TokenAdjustDefault = 0x0080;

    // SECURITY_IMPERSONATION_LEVEL and TOKEN_TYPE
    private const int SecurityImpersonation = 2;
    private const int TokenImpersonation = 1;

    // TOKEN_INFORMATION_CLASS
    private const int TokenIntegrityLevel = 25;

    private const uint SeGroupIntegrity = 0x00000020;
    private const uint SeGroupIntegrityEnabled = 0x00000040;

    private const int SeFileObject = 1;
    private const uint LabelSecurityInformation = 0x00000010;
    private const uint SddlRevision1 = 1;

    private const string MediumIntegritySid = "S-1-16-8192";

    // ML = mandatory label, OICI = inherited by files and subfolders, NW = no write-up, ME = Medium.
    // Writing a label at or below the caller's own level needs only WRITE_OWNER, so SeSecurityPrivilege
    // is not enabled.
    private const string MediumNoWriteUpLabel = "S:(ML;OICI;NW;;;ME)";

    public void CreateDirectory(string path)
    {
        using var mediumIntegrityToken = TryCreateMediumIntegrityToken();
        if (mediumIntegrityToken is null)
        {
            Directory.CreateDirectory(path);
            return;
        }

        try
        {
            WindowsIdentity.RunImpersonated(mediumIntegrityToken, () => Directory.CreateDirectory(path));
        }
        catch (SecurityException)
        {
            // thrown only when the impersonation itself fails; the directory is then created as usual
            Directory.CreateDirectory(path);
        }
    }

    public bool ApplyLabel(string path)
    {
        if (!ConvertStringSecurityDescriptorToSecurityDescriptor(MediumNoWriteUpLabel, SddlRevision1, out var securityDescriptor, out _))
            return false;

        using (securityDescriptor)
        {
            if (!GetSecurityDescriptorSacl(securityDescriptor, out var saclPresent, out var sacl, out _) || !saclPresent)
                return false;

            return SetNamedSecurityInfo(path, SeFileObject, LabelSecurityInformation, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, sacl) == 0;
        }
    }

    private static SafeAccessTokenHandle? TryCreateMediumIntegrityToken()
    {
        if (!OpenProcessToken(GetCurrentProcess(), TokenDuplicate | TokenQuery, out var processToken))
            return null;

        using (processToken)
        {
            // RunImpersonated duplicates the token it is given, so TOKEN_DUPLICATE is needed as well
            if (!DuplicateTokenEx(
                    processToken,
                    TokenDuplicate | TokenQuery | TokenImpersonate | TokenAdjustDefault,
                    IntPtr.Zero,
                    SecurityImpersonation,
                    TokenImpersonation,
                    out var impersonationToken))
                return null;

            if (TrySetMediumIntegrity(impersonationToken))
                return impersonationToken;

            impersonationToken.Dispose();
            return null;
        }
    }

    // Lowering the integrity level of a token needs no privilege; raising it would.
    private static bool TrySetMediumIntegrity(SafeAccessTokenHandle token)
    {
        var sid = new SecurityIdentifier(MediumIntegritySid);
        var sidBytes = new byte[sid.BinaryLength];
        sid.GetBinaryForm(sidBytes, 0);

        var sidHandle = GCHandle.Alloc(sidBytes, GCHandleType.Pinned);
        try
        {
            var label = new TokenMandatoryLabel
            {
                Label = new SidAndAttributes
                {
                    Sid = sidHandle.AddrOfPinnedObject(),
                    Attributes = SeGroupIntegrity | SeGroupIntegrityEnabled
                }
            };

            return SetTokenInformation(
                token,
                TokenIntegrityLevel,
                ref label,
                (uint)(Marshal.SizeOf<TokenMandatoryLabel>() + sidBytes.Length));
        }
        finally
        {
            sidHandle.Free();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SidAndAttributes
    {
        public IntPtr Sid;
        public uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TokenMandatoryLabel
    {
        public SidAndAttributes Label;
    }

    // Security descriptors returned by ConvertStringSecurityDescriptorToSecurityDescriptor are freed with LocalFree.
    private sealed class LocalAllocHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // required by the P/Invoke marshaller
        public LocalAllocHandle() : base(ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle() => LocalFree(handle) == IntPtr.Zero;

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr memory);
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out SafeAccessTokenHandle tokenHandle);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool DuplicateTokenEx(
        SafeAccessTokenHandle existingToken,
        uint desiredAccess,
        IntPtr tokenAttributes,
        int impersonationLevel,
        int tokenType,
        out SafeAccessTokenHandle newToken);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool SetTokenInformation(
        SafeAccessTokenHandle tokenHandle,
        int tokenInformationClass,
        ref TokenMandatoryLabel tokenInformation,
        uint tokenInformationLength);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(
        string stringSecurityDescriptor,
        uint stringSdRevision,
        out LocalAllocHandle securityDescriptor,
        out uint securityDescriptorSize);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool GetSecurityDescriptorSacl(
        LocalAllocHandle securityDescriptor,
        out bool saclPresent,
        out IntPtr sacl,
        out bool saclDefaulted);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
    private static extern uint SetNamedSecurityInfo(
        string objectName,
        int objectType,
        uint securityInfo,
        IntPtr owner,
        IntPtr group,
        IntPtr dacl,
        IntPtr sacl);
}

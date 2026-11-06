namespace AndroidDoctor.Native
{
    /// <summary>One native library found in the project, with where it came from.</summary>
    public sealed class NativeLib
    {
        public string Name;        // "libfoo.so"
        public string Abi;         // "arm64-v8a"; null if it could not be determined
        public string Location;    // asset path, or "<asset path of the .aar>!/jni/<abi>/<name>"
        public string Owner;       // "Packages/com.vendor.sdk", "Assets/Plugins/Vendor" or the .aar asset path
        public string ReadError;   // null if the ELF header was read
        public ElfInfo Elf;        // null if it could not be read

        public bool Is64Bit => Abi == "arm64-v8a" || Abi == "x86_64";
        public bool Is32Bit => Abi == "armeabi-v7a" || Abi == "x86" || Abi == "armeabi";
    }
}

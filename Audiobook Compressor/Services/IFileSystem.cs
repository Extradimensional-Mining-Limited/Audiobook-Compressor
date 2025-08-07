/*
    Filename: IFileSystem.cs
    Last Updated: 2025-08-07 12:43 CEST
    Version: 1.2.E
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Abstraction interface for file system operations enabling dependency injection and comprehensive unit testing of AudioProcessor file operations.
*/

using System.IO;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Abstraction for file system operations (copy, exists, directory creation)
    /// </summary>
    public interface IFileSystem
    {
        void Copy(string sourceFileName, string destFileName, bool overwrite);
        bool Exists(string path);
        void CreateDirectory(string path);
        long GetFileLength(string path);
    }

    /// <summary>
    /// Default implementation using System.IO
    /// </summary>
    public class DefaultFileSystem : IFileSystem
    {
        public void Copy(string sourceFileName, string destFileName, bool overwrite) => File.Copy(sourceFileName, destFileName, overwrite);
        public bool Exists(string path) => File.Exists(path);
        public void CreateDirectory(string path) => Directory.CreateDirectory(path);
        public long GetFileLength(string path) => new FileInfo(path).Length;
    }
}

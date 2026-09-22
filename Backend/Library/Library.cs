using System.Reflection.Metadata;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MusicPlayerApp
{
    public static class Library
    {
        public static string[] SUPPORTED_FORMATS =
        {
            ".mp3",
            ".wav",
            ".flac"
        };
        
        private static string[] GetFilePathsFromDirectory(string directoryPath)
        {
            string[] filePaths = Directory.GetFiles(directoryPath);
            string[] subdirectoryPaths = Directory.GetDirectories(directoryPath);
            foreach(string path in subdirectoryPaths)
            {
                string[] paths = GetFilePathsFromDirectory(path);
                filePaths = filePaths.Concat(paths).ToArray();
            }
            return filePaths;
        }
        
        // Filter out file paths not ending with .mp3, .wav, 
        // or other supported audio formats, see SUPPORTED_FORMATS
        private static string[] GetValidFilePaths(string[] filePaths)
        {
            return filePaths
                .Where(path => SUPPORTED_FORMATS.Contains(Path.GetExtension(path)))
                .ToArray();
        }
        
        public static void ImportFromPath(string path)
        {
            if (!Path.Exists(path))
                throw new ArgumentException($"Invalid path {path}");
            
            if(Directory.Exists(path))
            {
                string[] filePaths = GetFilePathsFromDirectory(path);
                string[] validFilePaths = GetValidFilePaths(filePaths);

                foreach(string p in validFilePaths)
                {
                    Console.WriteLine(p);
                }
            };
        }
    }
}
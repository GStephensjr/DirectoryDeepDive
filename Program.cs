using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryDeepDive
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Search();
            
        }
        public static void Search()
        {
            Console.WriteLine("Please enter source directory: ");
            string source = Console.ReadLine();
            if (!Directory.Exists(source)) 
            {
                throw new DirectoryNotFoundException($"Directory not found: {source}");
            }               

            Console.WriteLine("Please enter destination directory: ");
            string destination = Console.ReadLine();
            if (!Directory.Exists(destination))
            {
                throw new DirectoryNotFoundException($"Directory not found: {destination}");
            }

            Console.WriteLine("What are we looking for?");
            string query = Console.ReadLine();

            CopyMatchingFilesPreserveStructure(source, query, destination);
            Reset();
        }
        public static void Reset()
        {
            Console.WriteLine("press any button to continue...");
            var input = Console.ReadKey();
            Console.Clear();
            string[] args = new string[] { };
            Main(args);
        }
        public static void CopyMatchingFilesPreserveStructure(
                string sourceDirectory,
                string searchQuery,
                string destinationDirectory)
        {
            if (!Directory.Exists(sourceDirectory))
                throw new DirectoryNotFoundException(sourceDirectory);

            Directory.CreateDirectory(destinationDirectory);

            ProcessDirectory(sourceDirectory, sourceDirectory, searchQuery, destinationDirectory);
        }

        private static void ProcessDirectory(
            string currentDirectory,
            string rootSource,
            string searchQuery,
            string destinationRoot)
        {
            try
            {
                // Copy matching files in current directory
                foreach (var file in Directory.EnumerateFiles(currentDirectory))
                {
                    if (Path.GetFileName(file)
                        .IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string relativePath = file.Substring(rootSource.Length)
                                                  .TrimStart(Path.DirectorySeparatorChar);

                        string destinationPath = Path.Combine(destinationRoot, relativePath);

                        string destinationFolder = Path.GetDirectoryName(destinationPath);
                        if (!string.IsNullOrEmpty(destinationFolder))
                            Directory.CreateDirectory(destinationFolder);

                        File.Copy(file, destinationPath, true);
                    }
                }

                // Recurse into subdirectories
                foreach (var directory in Directory.EnumerateDirectories(currentDirectory))
                {
                    ProcessDirectory(directory, rootSource, searchQuery, destinationRoot);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip folders we don't have access to
            }
            catch (PathTooLongException)
            {
                // Skip invalid/long paths
            }
            catch (DirectoryNotFoundException)
            {

            }
        }

    }
}

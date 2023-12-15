using System;
using System.IO;
using OpenCompany.Postprocessor;
using OpenCompany.Preprocessor;
using OpenCompany.Shared;

namespace OpenCompany
{
    internal class Program
    {
        //D:\Steam\steamapps\common\Lethal Company
        //C:\Users\voxcake\Desktop\TestDecomp
        
        public static void Main(string[] args)
        {
            using var cancellationTokenSource = new ConsoleCancellationToken();

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (!TryRead("Enter the game path", out var gamePath))
                {
                    continue;
                }

                var managedFolder = Path.Combine(gamePath, "Lethal Company_Data/Managed");

                if (!Directory.Exists(managedFolder))
                {
                    Console.WriteLine($"Unable to found {managedFolder} folder, please write root game path");
                    
                    continue;
                }
                
                if (!TryRead("Enter the output solution path", out var solutionPath))
                {
                    continue;
                }
                
                if (!Directory.Exists(solutionPath))
                {
                    Directory.CreateDirectory(solutionPath);
                }
                
                var solutionAssembliesFolder = solutionPath.PathCombine("Assemblies");

                FileUtility.CopyFiles(managedFolder, solutionAssembliesFolder,
                    "dll", SearchOption.TopDirectoryOnly);

                PreprocessorEntryPoint.Main(new string[] { solutionAssembliesFolder });
                PostprocessorEntryPoint.Main(new string[] { solutionPath });
                
                Console.WriteLine("Project was generated!");
                Console.ReadLine();
            }
        }

        private static bool TryRead(string print, out string line)
        {
            Console.WriteLine(print);
                
            line = Console.ReadLine();

            if (string.IsNullOrEmpty(line))
            {
                var lowerCasePrint = print.LowercaseFirstChar();
                
                Console.WriteLine($"Wrong input, please {lowerCasePrint}");

                return false;
            }
            
            line = line
                .Replace("\\", "/")
                .Replace("\"", "");

            return true;
        }
    }
}
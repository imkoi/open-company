using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenCompany.Shared;

namespace OpenCompany.Postprocessor
{
    public class PostprocessorEntryPoint
    {
        public static void Main(string[] args)
        {
            var solutionFolder = args.First();
            var assemblyPath = solutionFolder.PathCombine("Assemblies").PathCombine("Assembly-CSharp.dll"); 
            
            var process = new Process
            { 
                StartInfo = new ProcessStartInfo
                { 
                    FileName = "cmd.exe",
                    Arguments = $"/C ilspycmd -p {assemblyPath} -o {solutionFolder}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                var files = Directory.GetFiles(solutionFolder, "*.cs", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var content = File.ReadAllText(file);
                    var newContent = Regex.Replace(content,
                        @"protected internal override string __getTypeName\(\)", 
                        "protected override string __getTypeName()");
                    File.WriteAllText(file, newContent);
                }
            }
        }
    }
}
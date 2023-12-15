using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace OpenCompany.Preprocessor
{
    internal class PreprocessorAssemblyResolver : IAssemblyResolver
    {
        private readonly Dictionary<string, AssemblyDefinition> _assemblyCache;
        private readonly HashSet<string> _assemblyDirectories;
        private string _currentAssemblyName;
        private AssemblyDefinition _currentAssemblyDefinition;
        private ReaderParameters _currentAssemblyReaderParameters;

        public PreprocessorAssemblyResolver(string assembliesDirectory)
        {
            _assemblyDirectories = new HashSet<string>();
            _assemblyCache = new Dictionary<string, AssemblyDefinition>();

            _assemblyDirectories.Add(assembliesDirectory);
        }
        
        public void SetCurrentAssemblyPath(string assemblyPath)
        {
            var assemblyForwardSlashLocation = assemblyPath.Replace('\\', '/');
            
            var splittedAssemblyPath = assemblyForwardSlashLocation.Split('/');

            var assembliesFolder = splittedAssemblyPath[0] + '/';

            for (var i = 1; i < splittedAssemblyPath.Length - 1; i++)
            {
                assembliesFolder += splittedAssemblyPath[i] + '/';
            }
            
            _currentAssemblyName = splittedAssemblyPath.Last().Replace(".dll", "");
            _assemblyDirectories.Add(assembliesFolder);
        }

        public void SetCurrentAssembly(AssemblyDefinition assemblyDefinition)
        {
            _currentAssemblyDefinition = assemblyDefinition;
        }

        public void SetReaderParameters(ReaderParameters readerParameters)
        {
            _currentAssemblyReaderParameters = readerParameters;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference assemblyReference) => Resolve(assemblyReference, _currentAssemblyReaderParameters);

        public AssemblyDefinition Resolve(AssemblyNameReference assemblyReference, ReaderParameters parameters)
        {
            lock (_assemblyCache)
            {
                if (assemblyReference.Name == _currentAssemblyName)
                {
                    return _currentAssemblyDefinition;
                }

                var fileName = FindFile(assemblyReference);
                
                if (_assemblyCache.TryGetValue(assemblyReference.Name, out var result))
                {
                    return result;
                }

                var readerParameters = new ReaderParameters
                {
                    ReadSymbols = true,
                    ReadingMode = parameters.ReadingMode,
                    SymbolStream = parameters.SymbolStream,
                    SymbolReaderProvider = new PortablePdbReaderProvider(),
                    AssemblyResolver = parameters.AssemblyResolver
                };

                readerParameters.AssemblyResolver = this;

                var ms = MemoryStreamFor(fileName);
                var pdb = fileName.Replace(".dll", ".pdb");
                if (File.Exists(pdb))
                {
                    readerParameters.SymbolStream = MemoryStreamFor(pdb);
                }
                else
                {
                    readerParameters.ReadSymbols = false;
                    readerParameters.SymbolStream = null;
                    readerParameters.SymbolReaderProvider = null;
                }
                
                var assemblyDefinition = AssemblyDefinition.ReadAssembly(ms, readerParameters);
                _assemblyCache.Add(assemblyReference.Name, assemblyDefinition);

                return assemblyDefinition;
            }
        }

        private string FindFile(AssemblyNameReference assemblyReference)
        {
            foreach (var assembliesPath in _assemblyDirectories)
            {
                var assemblyFileName = Path.Combine(assembliesPath, assemblyReference.Name + ".dll");

                if (File.Exists(assemblyFileName))
                {
                    return assemblyFileName;
                }
            }

            throw new IOException($"Failed to find assembly for reference {assemblyReference.Name}");
        }
        
        public void Dispose()
        {
            _assemblyDirectories.Clear();
            _assemblyCache.Clear();
        }

        private static MemoryStream MemoryStreamFor(string fileName)
        {
            return Retry(10, TimeSpan.FromSeconds(1), () =>
            {
                byte[] byteArray;
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byteArray = new byte[fs.Length];
                    var readLength = fs.Read(byteArray, 0, (int)fs.Length);
                    if (readLength != fs.Length)
                    {
                        throw new InvalidOperationException("File read length is not full length of file.");
                    }
                }

                return new MemoryStream(byteArray);
            });
        }

        private static MemoryStream Retry(int retryCount, TimeSpan waitTime, Func<MemoryStream> func)
        {
            try
            {
                return func();
            }
            catch (IOException)
            {
                if (retryCount == 0)
                {
                    throw;
                }

                Console.WriteLine($"Caught IO Exception, trying {retryCount} more times");
                Thread.Sleep(waitTime);

                return Retry(retryCount - 1, waitTime, func);
            }
        }
    }
}
using System;
using System.IO;
using Mono.Cecil;

namespace OpenCompany.Preprocessor
{
    internal class PreprocessorOperation : IDisposable
    {
        private readonly string _assemblyPath;
        private readonly PreprocessorAssemblyResolver _assemblyResolver;
        
        private FileStream _peFile;
        private AssemblyDefinition _assemblyDefinition;

        private PreprocessorOperation(string assemblyPath, PreprocessorAssemblyResolver assemblyResolver)
        {
            _assemblyPath = assemblyPath;
            _assemblyResolver = assemblyResolver;
        }
        
        public static PreprocessorOperation Create(string assemblyPath, PreprocessorAssemblyResolver assemblyResolver)
        {
            var operation = new PreprocessorOperation(assemblyPath, assemblyResolver);
            
            operation._peFile = File.Open(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            
            operation._assemblyResolver.SetCurrentAssemblyPath(assemblyPath);
            
            var readerParameters = new ReaderParameters
            {
                ReadSymbols = false,
                ReadingMode = ReadingMode.Immediate,
                AssemblyResolver = operation._assemblyResolver
            };

            operation._assemblyResolver.SetReaderParameters(readerParameters);
            
            operation._assemblyDefinition = AssemblyDefinition.ReadAssembly(operation._peFile, readerParameters);
            operation._assemblyResolver.SetCurrentAssembly(operation._assemblyDefinition);

            return operation;
        }

        private void CloseAssembly()
        {
            var pe = new MemoryStream((int) _peFile.Length);

            var writerParameters = new WriterParameters
            {
                WriteSymbols = false,
            };

            _assemblyDefinition.Write(pe, writerParameters);

            _peFile.Dispose();

            File.WriteAllBytes(_assemblyPath, pe.ToArray());
        }
        
        public void Dispose()
        {
            CloseAssembly();
        }

        public void Patch(Action<AssemblyDefinition> process)
        {
            process?.Invoke(_assemblyDefinition);
        }
    }
}
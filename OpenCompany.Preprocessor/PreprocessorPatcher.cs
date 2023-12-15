using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;

namespace OpenCompany.Preprocessor
{
    internal class PreprocessorPatcher : IDisposable
    {
        private readonly string _assemblyPath;
        private readonly PreprocessorAssemblyResolver _assemblyResolver;

        private List<PreprocessorOperation> _patchOperations;

        public PreprocessorPatcher(string assemblyPath)
        {
            var assembliesDirectory = Path.GetDirectoryName(assemblyPath);
            
            _assemblyPath = assemblyPath;
            _assemblyResolver = new PreprocessorAssemblyResolver(assembliesDirectory);
            _patchOperations = new List<PreprocessorOperation>();
        }
        
        public void Patch(
            Action<AssemblyDefinition> process)
        {
            var operation = PreprocessorOperation.Create(_assemblyPath, _assemblyResolver);

            operation.Patch(process);
 
            _patchOperations.Add(operation);
        }

        void IDisposable.Dispose()
        {
            foreach (var patchOperation in _patchOperations)
            {
                patchOperation.Dispose();
            }
            
            _patchOperations.Clear();
            _assemblyResolver?.Dispose();
        }
    }
}
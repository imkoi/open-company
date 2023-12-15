using System;
using System.Linq;
using Mono.Cecil;
using OpenCompany.Shared;

namespace OpenCompany.Preprocessor
{
    public class PreprocessorEntryPoint
    {
        public static void Main(string[] args)
        {
            var solutionManagedFolder = args.First();
            var assemblyPath = solutionManagedFolder.PathCombine("Unity.Netcode.Runtime.dll");
            
            using var patcher = new PreprocessorPatcher(assemblyPath);
                
            patcher.Patch(Process);
        }
        
        private static void Process(AssemblyDefinition assemblyDefinition)
        {
            var module = assemblyDefinition.MainModule;
            
            foreach (var type in module.Types)
            {
                if (type.Name.Equals("NetworkBehaviour"))
                {
                    var getTypeNameMethod = type.Methods.First(method => method.Name == "__getTypeName");
                    var rpcExecStageField = type.Fields.First(field => field.Name == "__rpc_exec_stage");
                    
                    getTypeNameMethod.Attributes &= ~MethodAttributes.Assembly;
                    getTypeNameMethod.Attributes &= ~MethodAttributes.CheckAccessOnOverride;
                    rpcExecStageField.Attributes &= ~FieldAttributes.Assembly;
                    rpcExecStageField.Attributes |= FieldAttributes.Public;
                    
                    Console.WriteLine($"Processing type {type.FullName}");
                    Console.WriteLine($"-Removed internal modificator for {getTypeNameMethod.FullName}");
                    Console.WriteLine($"-Removed internal modificator for {rpcExecStageField.FullName}");
                }
            }
        }
    }
}
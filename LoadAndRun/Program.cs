using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoadAndRun
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var path = args.Length == 1 ? args[0] : @"D:\Programming\Projects\LoadAndRun\ManipulationTest\bin\Debug\ManipulationTest.exe";

            var assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { ReadSymbols = true });
            var module = assembly.MainModule;
            foreach (var reference in module.AssemblyReferences)
                Console.WriteLine(reference);

            var testType = module.Types.Single(typeDef => typeDef.Name == "Print");

            var hasInitialized = new FieldDefinition("hasInitialized", FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly, module.TypeSystem.Boolean);
            testType.Fields.Add(hasInitialized);

            var testCctor = testType.Methods.SingleOrDefault(funcDef => funcDef.Name == ".cctor");
            testCctor.IsAssembly = true;

            var firstTestCctorInstruction = testCctor.Body.Instructions.First();
            var il = testCctor.Body.GetILProcessor();

            il.InsertBefore(firstTestCctorInstruction, il.Create(OpCodes.Ldsfld, hasInitialized));

            var afterIf = il.Create(OpCodes.Ldc_I4_1);
            var insideIf = il.Create(OpCodes.Ret);
            il.InsertBefore(firstTestCctorInstruction, il.Create(OpCodes.Brtrue_S, insideIf));
            il.InsertBefore(firstTestCctorInstruction, il.Create(OpCodes.Br, afterIf));
            il.InsertBefore(firstTestCctorInstruction, insideIf);
            il.InsertBefore(firstTestCctorInstruction, afterIf);
            il.InsertBefore(firstTestCctorInstruction, il.Create(OpCodes.Stsfld, hasInitialized));

            testCctor.Body.OptimizeMacros();

            var cctorAttributes = MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig;
            var cctor = new MethodDefinition(".cctor", cctorAttributes, module.TypeSystem.Void);
            var cctorIL = cctor.Body.GetILProcessor();
            cctorIL.Append(cctorIL.Create(OpCodes.Call, testCctor));
            cctorIL.Append(cctorIL.Create(OpCodes.Ret));

            module.Types.Single(typeRef => typeRef.Name == "<Module>").Methods.Add(cctor);

            assembly.Write(path, new WriterParameters { WriteSymbols = true });

            Console.WriteLine("Injection Done!");
        }
    }
}
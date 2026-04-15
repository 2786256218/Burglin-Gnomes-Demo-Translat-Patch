using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

class Patcher
{
    static void Main(string[] args)
    {
        string managedDir = Directory.GetCurrentDirectory();
        if (!File.Exists(Path.Combine(managedDir, "Unity.TextMeshPro.dll")))
        {
            managedDir = Path.Combine(Directory.GetCurrentDirectory(), "Gnomium_Data", "Managed");
        }
        
        string tmpDllPath = Path.Combine(managedDir, "Unity.TextMeshPro.dll");
        string backupTmpDllPath = Path.Combine(managedDir, "Unity.TextMeshPro.dll.bak");
        
        string myDllPath = Path.Combine(managedDir, "GnomiumTranslationCore.dll");
        
        Console.WriteLine("Patcher started...");

        PatchAssembly(managedDir, "Unity.TextMeshPro.dll", myDllPath, "GnomiumTranslation.TranslationCore", "OnEnableTMPText", new[] { "TMPro.TextMeshPro", "TMPro.TextMeshProUGUI" });
        PatchAssembly(managedDir, "UnityEngine.UI.dll", myDllPath, "GnomiumTranslation.TranslationCore", "OnEnableText", new[] { "UnityEngine.UI.Text" });
        PatchAssembly(managedDir, "UnityEngine.UIElementsModule.dll", myDllPath, "GnomiumTranslation.TranslationCore", "OnEnableUIElementsText", new[] { "UnityEngine.UIElements.TextElement" });
        PatchPlayerController(managedDir, myDllPath);

        Console.WriteLine("Patcher finished successfully!");
    }

    static void PatchPlayerController(string managedDir, string myDllPath)
    {
        string targetDllPath = Path.Combine(managedDir, "Assembly-CSharp.dll");
        string backupDllPath = targetDllPath + ".bak";
        
        if (!File.Exists(backupDllPath))
        {
            Console.WriteLine(string.Format("Backing up Assembly-CSharp.dll..."));
            File.Copy(targetDllPath, backupDllPath);
        }
        else
        {
            Console.WriteLine("Restoring Assembly-CSharp.dll from backup...");
            File.Copy(backupDllPath, targetDllPath, true);
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managedDir);

        var readerParams = new ReaderParameters { AssemblyResolver = resolver, ReadWrite = true };

        using (var assembly = AssemblyDefinition.ReadAssembly(targetDllPath, readerParams))
        {
            var myAssembly = AssemblyDefinition.ReadAssembly(myDllPath);
            var myType = myAssembly.MainModule.GetType("GnomiumTranslation.TranslationCore");
            if (myType == null) throw new Exception("GnomiumTranslation.TranslationCore not found");
            
            var myMethod = myType.Methods.FirstOrDefault(m => m.Name == "GetDayText");
            if (myMethod == null) throw new Exception("GetDayText not found");
            
            var translateMethod = myType.Methods.FirstOrDefault(m => m.Name == "Translate");
            if (translateMethod == null) throw new Exception("Translate not found");

            var importedMethod = assembly.MainModule.ImportReference(myMethod);
            var importedTranslateMethod = assembly.MainModule.ImportReference(translateMethod);

            var type = assembly.MainModule.GetType("PlayerController");
            if (type != null)
            {
                var targetMethod = type.Methods.FirstOrDefault(m => m.Name == "DisplayDay");
                if (targetMethod != null)
                {
                    var ilProcessor = targetMethod.Body.GetILProcessor();
                    for (int i = 0; i < targetMethod.Body.Instructions.Count; i++)
                    {
                        var inst = targetMethod.Body.Instructions[i];
                        if (inst.OpCode == OpCodes.Callvirt)
                        {
                            var methodRef = inst.Operand as MethodReference;
                            if (methodRef != null && methodRef.Name == "set_text")
                            {
                                var callGetDayText = Instruction.Create(OpCodes.Call, importedMethod);
                                ilProcessor.InsertBefore(inst, callGetDayText);
                                Console.WriteLine("Injected GetDayText into PlayerController.DisplayDay");
                                break;
                            }
                        }
                    }
                }
                
                var targetMinutesMethod = type.Methods.FirstOrDefault(m => m.Name == "Instance_onMinutesLeft");
                if (targetMinutesMethod != null)
                {
                    var ilProcessor = targetMinutesMethod.Body.GetILProcessor();
                    for (int i = 0; i < targetMinutesMethod.Body.Instructions.Count; i++)
                    {
                        var inst = targetMinutesMethod.Body.Instructions[i];
                        if (inst.OpCode == OpCodes.Callvirt)
                        {
                            var methodRef = inst.Operand as MethodReference;
                            if (methodRef != null && methodRef.Name == "set_text")
                            {
                                var callTranslateText = Instruction.Create(OpCodes.Call, importedTranslateMethod);
                                ilProcessor.InsertBefore(inst, callTranslateText);
                                Console.WriteLine("Injected Translate into PlayerController.Instance_onMinutesLeft");
                                i++; // Skip the newly inserted instruction
                            }
                        }
                    }
                }
            }

            assembly.Write(targetDllPath + ".patched");
        }
        
        File.Delete(targetDllPath);
        File.Move(targetDllPath + ".patched", targetDllPath);
    }

    static void PatchAssembly(string managedDir, string dllName, string myDllPath, string myTypeName, string myMethodName, string[] typesToPatch)
    {
        string targetDllPath = Path.Combine(managedDir, dllName);
        string backupDllPath = targetDllPath + ".bak";
        
        if (!File.Exists(backupDllPath))
        {
            Console.WriteLine(string.Format("Backing up {0}...", dllName));
            File.Copy(targetDllPath, backupDllPath);
        }
        else
        {
            Console.WriteLine(string.Format("Restoring {0} from backup...", dllName));
            File.Copy(backupDllPath, targetDllPath, true);
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managedDir);

        var readerParams = new ReaderParameters { AssemblyResolver = resolver, ReadWrite = true };

        using (var assembly = AssemblyDefinition.ReadAssembly(targetDllPath, readerParams))
        {
            var myAssembly = AssemblyDefinition.ReadAssembly(myDllPath);
            var myType = myAssembly.MainModule.GetType(myTypeName);
            if (myType == null) throw new Exception(string.Format("{0} not found", myTypeName));
            var myMethod = myType.Methods.FirstOrDefault(m => m.Name == myMethodName);
            if (myMethod == null) throw new Exception(string.Format("{0} not found", myMethodName));
            
            var getTranslatedStringMethod = myType.Methods.FirstOrDefault(m => m.Name == "GetTranslatedString");
            if (getTranslatedStringMethod == null) throw new Exception("GetTranslatedString not found");

            var importedMethod = assembly.MainModule.ImportReference(myMethod);
            var importedGetMethod = assembly.MainModule.ImportReference(getTranslatedStringMethod);

            // Support patching the base class TMP_Text if typesToPatch includes TMPro.TextMeshPro
            var actualTypesToPatch = typesToPatch.ToList();
            if (actualTypesToPatch.Contains("TMPro.TextMeshPro")) {
                actualTypesToPatch.Add("TMPro.TMP_Text");
            }

            foreach (var typeName in actualTypesToPatch)
            {
                var type = assembly.MainModule.GetType(typeName);
                if (type == null) continue;
                
                var targetMethod = type.Methods.FirstOrDefault(m => m.Name == "OnEnable");
                if (targetMethod == null) targetMethod = type.Methods.FirstOrDefault(m => m.Name == "Awake");
                if (targetMethod == null) targetMethod = type.Methods.FirstOrDefault(m => m.Name == "Start");
                
                if (targetMethod != null)
                {
                    var ilProcessor = targetMethod.Body.GetILProcessor();
                    var firstInstruction = targetMethod.Body.Instructions[0];
                    
                    var ldarg0 = Instruction.Create(OpCodes.Ldarg_0);
                    var call = Instruction.Create(OpCodes.Call, importedMethod);
                    
                    ilProcessor.InsertBefore(firstInstruction, ldarg0);
                    ilProcessor.InsertAfter(ldarg0, call);
                    Console.WriteLine(string.Format("Injected {0} into {1}.{2} in {3}", myMethodName, typeName, targetMethod.Name, dllName));
                }
                
                // 拦截 text 的 setter，防止动态更新的文本未被汉化
                var textSetter = type.Methods.FirstOrDefault(m => m.Name == "set_text");
                if (textSetter != null)
                {
                    var ilProcessor = textSetter.Body.GetILProcessor();
                    var firstInstruction = textSetter.Body.Instructions[0];
                    
                    // 将参数 (value) 传入 GetTranslatedString，并将返回值赋给 value
                    var ldarg1 = Instruction.Create(OpCodes.Ldarg_1);
                    var callGet = Instruction.Create(OpCodes.Call, importedGetMethod);
                    var starg1 = Instruction.Create(OpCodes.Starg_S, textSetter.Parameters[0]);
                    
                    ilProcessor.InsertBefore(firstInstruction, ldarg1);
                    ilProcessor.InsertAfter(ldarg1, callGet);
                    ilProcessor.InsertAfter(callGet, starg1);
                    
                    Console.WriteLine(string.Format("Injected GetTranslatedString into {0}.set_text in {1}", typeName, dllName));
                }

                // 拦截 SetText 方法
                var setTextMethod = type.Methods.FirstOrDefault(m => m.Name == "SetText" && m.Parameters.Count > 0 && m.Parameters[0].ParameterType.FullName == "System.String");
                if (setTextMethod != null)
                {
                    var ilProcessor = setTextMethod.Body.GetILProcessor();
                    var firstInstruction = setTextMethod.Body.Instructions[0];
                    
                    var ldarg1 = Instruction.Create(OpCodes.Ldarg_1);
                    var callGet = Instruction.Create(OpCodes.Call, importedGetMethod);
                    var starg1 = Instruction.Create(OpCodes.Starg_S, setTextMethod.Parameters[0]);
                    
                    ilProcessor.InsertBefore(firstInstruction, ldarg1);
                    ilProcessor.InsertAfter(ldarg1, callGet);
                    ilProcessor.InsertAfter(callGet, starg1);
                    
                    Console.WriteLine(string.Format("Injected GetTranslatedString into {0}.SetText in {1}", typeName, dllName));
                }
            }

            assembly.Write(targetDllPath + ".patched");
        }
        
        File.Delete(targetDllPath);
        File.Move(targetDllPath + ".patched", targetDllPath);
    }
}
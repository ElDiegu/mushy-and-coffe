using System;
using System.Collections.Generic;
using System.Reflection;

namespace EventSystem
{
	public class PredefinedAssemblyUtil
	{
		enum AssemblyType 
		{
			AssemblyCSharp,
			AssemblyCSharpFirstPass,
			AssemblyCSharpEditor,
			AssemblyCSharpEditorFirstPass
		}
		
		static AssemblyType? GetAssemblyType(string assemblyName) 
		{
			return assemblyName switch 
			{
				"Assembly-CSharp" => AssemblyType.AssemblyCSharp,
				"Assembly-CSharp-firstpass" => AssemblyType.AssemblyCSharpFirstPass,
				"Assembly-CSharp-Editor" => AssemblyType.AssemblyCSharpEditor,
				"Assembly-CSharp-Editor-firstpass" => AssemblyType.AssemblyCSharpEditorFirstPass,
				_ => null
			};
		}
		
		static void AddTypesFromAssembly(Type[] assembly, ICollection<Type> types, Type interfaceType) 
		{
			if (assembly == null) return;
			
			for (int i = 0; i < assembly.Length; i++) 
			{
				Type type = assembly[i];
				if (type != interfaceType && interfaceType.IsAssignableFrom(type)) 
				{
					types.Add(type);
				}
			}
		}
		
		public static List<Type> GetTypes(Type interfaceType) 
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			
			Dictionary<AssemblyType, Type[]> assemblyTypes = new Dictionary<AssemblyType, Type[]>();
			List<Type> types = new List<Type>();
			for (int i = 0; i < assemblies.Length; i++) 
			{
				AssemblyType? assemblyType = GetAssemblyType(assemblies[i].GetName().Name);
				
				if (assemblyType != null) 
				{
					assemblyTypes.Add((AssemblyType) assemblyType, assemblies[i].GetTypes());
				}
			}
			
			AddTypesFromAssembly(assemblyTypes[AssemblyType.AssemblyCSharp], types, interfaceType);
			if (assemblyTypes.ContainsKey(AssemblyType.AssemblyCSharpFirstPass)) AddTypesFromAssembly(assemblyTypes[AssemblyType.AssemblyCSharpFirstPass], types, interfaceType);
			
			return types;
		}
	}
}

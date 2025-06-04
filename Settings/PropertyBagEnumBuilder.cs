using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Settings
{
    public class PropertyBagEnumBuilder
    {
        public static Enum CreateEnum(string[] items)
        {
            AppDomain domain = Thread.GetDomain();
            AssemblyName name = new AssemblyName();
            name.Name = "EnumAssembly";
            AssemblyBuilder asmBuilder = domain.DefineDynamicAssembly(
                name, AssemblyBuilderAccess.Run);
            ModuleBuilder modBuilder =
                asmBuilder.DefineDynamicModule("EnumModule");
            EnumBuilder enumBuilder = modBuilder.DefineEnum("Language",
                                                            TypeAttributes.Public,
                                                            typeof (Int32));
            
            for (int i = 0; i < items.Length; i++)
            {
                // here is an array list with a list of string values
                enumBuilder.DefineLiteral(items[i], i);
            }
            //return  enumBuilder.CreateType();
            
            
            // create an instance
            return (Enum)Activator.CreateInstance(enumBuilder.CreateType());
        }
    }
}
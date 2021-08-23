using System;
using System.Runtime.Serialization;

namespace Kelary.Report
{
    /// <summary>
    /// Deserialization type binder class.
    /// Convert types for backward compatibility.
    /// </summary>
    internal class TypeNameConverter : SerializationBinder
    {
        private SerializationBinder Binder;
        public TypeNameConverter(SerializationBinder Binder)
        {
            this.Binder = Binder;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = "";
            typeName = "";
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            var newTypeName = typeName;
            // Replace types for backward compatibility.

            if (assemblyName.Contains("CompoundDocument"))
            {
                assemblyName = typeof(CompoundDocument).Assembly.FullName;
                typeName = typeName.Replace("CompoundDocument.CompoundDocument+DocumentData", "Kelary.Report.CompoundDocument+DocumentData");  // typeof(CompoundDocument.DocumentData).FullName
            }
            if (Binder != null)
                return Binder?.BindToType(assemblyName, typeName);
            return Type.GetType(string.Format("{0}, {1}", newTypeName, assemblyName));
        }
    }
}

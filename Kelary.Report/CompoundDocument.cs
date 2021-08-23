using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace Kelary.Report
{
    public class CompoundDocument
    {
        [Serializable]
        private class DocumentData
        {
            public string Xaml { get; set; }

            public Dictionary<string, ICloneable> PersitedValues;

            public Dictionary<string, Type> PersitedTypes;
        }

        public Dictionary<string, ICloneable> PersitedValues = new Dictionary<string, ICloneable>();

        public Dictionary<string, Type> PersitedTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Save FlowDocument to specified file.
        /// </summary>
        /// <param name="document">FlowDocument object to save.</param>
        /// <param name="filePath"></param>
        /// <param name="Factory"></param>
        public void Save(FlowDocument document, string filePath, EnhancedUserControlFactory Factory)
        {
            var documentData = new DocumentData
            {
                PersitedTypes = PersitedTypes,
                PersitedValues = PersitedValues,
                Xaml = XamlWriter.Save(document)
            };
            documentData.Xaml = EnhancedUserControlUtils.ReplaceControlsWithPlaceholders(documentData.Xaml, document.ContentStart, document.ContentEnd, Factory);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                using (var gzipStream = new GZipStream(fileStream, CompressionMode.Compress))
                {
                    var binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(gzipStream, documentData);
                }
            }
        }

        /// <summary>
        /// Load FlowDocument from file.
        /// Binder used to resolve datatypes.
        /// </summary>
        /// <param name="filePath">Path to FlowDocument file.</param>
        /// <param name="Factory"></param>
        /// <param name="Binder"></param>
        /// <returns></returns>
        public FlowDocument Load(string filePath, EnhancedUserControlFactory Factory, SerializationBinder Binder)
        {
            // Converter for backward compatibility
            var CompatibilityBinder = new TypeNameConverter(Binder);
            DocumentData documentData;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter
                {
                    Binder = CompatibilityBinder
                };
                documentData = binaryFormatter.Deserialize(gzipStream) as DocumentData;
            }

            FlowDocument document;


            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamWriter streamWriter = new StreamWriter(memoryStream))
            {
                string assembly = typeof(PlaceholderControl).Assembly.GetName().Name;
                string clrns = typeof(PlaceholderControl).Namespace;
                string xmlns = EnhancedUserControlUtils.ShortNamespace(clrns);
                string xmlns1 = EnhancedUserControlUtils.ShortNamespace("CompoundDocument");

                string xaml = documentData.Xaml;

                // Backward compatibility
                xaml = xaml.Replace("cd:PlaceholderControl", "kr:PlaceholderControl");
                streamWriter.WriteLine(xaml);
                streamWriter.Flush();

                memoryStream.Position = 0;

                // Add namespace for  PlaceholderControl
                var parserContext = new ParserContext();
                parserContext.XmlnsDictionary.Add(xmlns, string.Format("clr-namespace:{0};assembly={1}", clrns, assembly));

                // Load document
                document = (FlowDocument)XamlReader.Load(memoryStream, parserContext);

                // Replace persisted types with correct ones.
                // Any persisted types used before should be declared as
                // Serialization Binder does not work for Type objects.
                PersitedTypes = documentData.PersitedTypes
                    .ToDictionary(item => item.Key, item => Binder.BindToType(item.Value.Assembly.FullName, item.Value.FullName));
                PersitedValues = documentData.PersitedValues;
            }

            EnhancedUserControlUtils.ReplacePlaceholdersWithRealControls(document, Factory, this);
            #region Get rid of unused PersistedTypes and PersistedValues.

            // Scan through the document and get all IDs for EnhancedUserControls.

            TextPointer current = document.ContentStart;

            Dictionary<string, bool> currentControlNames = new Dictionary<string, bool>();

            while (current.CompareTo(document.ContentEnd) < 0)
            {
                if (current.Parent is BlockUIContainer || current.Parent is InlineUIContainer)
                {
                    string containerMarkup = XamlWriter.Save(current.Parent);

                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(containerMarkup);

                    XmlAttribute nameAttribute = xmlDocument.DocumentElement.FirstChild.Attributes["Name"];

                    string name = null;

                    if (nameAttribute != null && !string.IsNullOrEmpty(nameAttribute.Value))
                    {
                        name = nameAttribute.Value;

                        if (!currentControlNames.ContainsKey(name))
                        {
                            currentControlNames.Add(name, true);
                        }
                    }
                }

                current = current.GetNextContextPosition(LogicalDirection.Forward);
            }

            // Delete unused persisted types.
            for (int i = PersitedTypes.Keys.Count - 1; i >= 0; i--)
            {
                string key = PersitedTypes.Keys.ElementAt(i);

                if (!currentControlNames.ContainsKey(key))
                {
                    PersitedTypes.Remove(key);
                }
            }

            // Delete unused persisted values.
            for (int i = PersitedValues.Keys.Count - 1; i >= 0; i--)
            {
                string key = PersitedValues.Keys.ElementAt(i);

                if (!currentControlNames.ContainsKey(key))
                {
                    PersitedValues.Remove(key);
                }
            }

            #endregion
            return document;
        }
    }
}

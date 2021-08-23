using System.IO;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;

namespace Kelary.Report
{
    /// <summary>
    /// Based on https://docs.microsoft.com/en-us/archive/blogs/fyuan/convert-xaml-flow-document-to-xps-with-style-multiple-page-page-size-header-margin
    /// </summary>
    public static class Print
    {
        public static string SaveAsXps(Stream stream, string header, Typeface typeFace, double emSize, EnhancedUserControlFactory Factory, CompoundDocument CompoundDocument)
        {
            string tempFileName = Path.GetTempFileName();

            MemoryStream placeholderStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(placeholderStream);

            using (StreamReader streamReader = new StreamReader(stream))
            {
                string xaml = streamReader.ReadToEnd();
                xaml = EnhancedUserControlUtils.ReplaceControlsWithMarkup(xaml, Factory, CompoundDocument);

                streamWriter.WriteLine(xaml);

                streamWriter.Flush();
                placeholderStream.Position = 0;
            }

            // Save as RTF
            FlowDocument obj = (FlowDocument)System.Windows.Markup.XamlReader.Load(placeholderStream);
            /*
			using (FileStream fs = new FileStream("D:\\test.rtf", FileMode.OpenOrCreate, FileAccess.ReadWrite))
			{
				TextRange textRange = new TextRange(obj.ContentStart, obj.ContentEnd);
				textRange.Save(fs, DataFormats.Rtf);
			}*/
            IDocumentPaginatorSource doc = (IDocumentPaginatorSource)obj;

            streamWriter.Close();
            placeholderStream.Close();

            using (Package container = Package.Open(tempFileName, FileMode.Create))
            using (XpsDocument xpsDoc = new XpsDocument(container, CompressionOption.Fast))
            {
                XpsSerializationManager rsm = new XpsSerializationManager(new XpsPackagingPolicy(xpsDoc), false);

                DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;

                // 8.5" X 11", with 0.5" margin.
                const int inch = 96;

                paginator = new DocumentPaginatorWrapper(paginator, new Size(inch * 8.5, inch * 11), new Size(inch * 0.5, inch * 0.5), header, typeFace, emSize);

                rsm.SaveAsXaml(paginator);
            }

            return tempFileName;
        }

        /// <summary>
        /// Save report to RTF file.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="stream"></param>
        /// <param name="Factory"></param>
        /// <param name="CompoundDocument"></param>
        private static void SaveStream(string Path, Stream stream, EnhancedUserControlFactory Factory, CompoundDocument CompoundDocument, string Format = "Rich Text Format")
        {
            MemoryStream placeholderStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(placeholderStream);

            using (StreamReader streamReader = new StreamReader(stream))
            {
                string xaml = streamReader.ReadToEnd();
                xaml = EnhancedUserControlUtils.ReplaceControlsWithMarkup(xaml, Factory, CompoundDocument);

                streamWriter.WriteLine(xaml);

                streamWriter.Flush();
                placeholderStream.Position = 0;
            }

            // Save as RTF
            FlowDocument obj = (FlowDocument)System.Windows.Markup.XamlReader.Load(placeholderStream);

            using (FileStream fs = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                TextRange textRange = new TextRange(obj.ContentStart, obj.ContentEnd);
                textRange.Save(fs, Format);
            }
        }

        /// <summary>
        /// Save FlowDocument to RTF file.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="document"></param>
        /// <param name="Factory"></param>
        /// <param name="CompoundDocument"></param>
        public static void Save(string Path, FlowDocument document, EnhancedUserControlFactory Factory, CompoundDocument CompoundDocument, string Format = "Rich Text Format")
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                System.Windows.Markup.XamlWriter.Save(document, memoryStream);
                memoryStream.Flush();
                memoryStream.Position = 0;

                SaveStream(Path, memoryStream, Factory, CompoundDocument, Format);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="header"></param>
        /// <param name="typeface"></param>
        /// <param name="emSize"></param>
        /// <param name="Factory"></param>
        /// <param name="CompoundDocument"></param>
        /// <returns></returns>
        private static string ConvertToXPSDocument(FlowDocument document, string header, Typeface typeface, double emSize, EnhancedUserControlFactory Factory, CompoundDocument CompoundDocument)
        {
            string documentPath = null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                System.Windows.Markup.XamlWriter.Save(document, memoryStream);
                memoryStream.Flush();
                memoryStream.Position = 0;

                documentPath = SaveAsXps(memoryStream, header, typeface, emSize, Factory, CompoundDocument);
            }

            return documentPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="header"></param>
        /// <param name="typeface"></param>
        /// <param name="emSize"></param>
        /// <param name="Factory"></param>
        /// <param name="CompoundDocument"></param>
        public static void PrintDocument(FlowDocument document, string header, Typeface typeface, double emSize, EnhancedUserControlFactory Factory, CompoundDocument CompoundDocument)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            string documentPath = ConvertToXPSDocument(document, header, typeface, emSize, Factory, CompoundDocument);

            using (XpsDocument xpsDocument = new XpsDocument(documentPath, FileAccess.Read))
            {
                PrintDialog printDialog = new PrintDialog();

                Mouse.OverrideCursor = null;

                if (printDialog.ShowDialog().GetValueOrDefault())
                {
                    FixedDocumentSequence fixedDocSeq = xpsDocument.GetFixedDocumentSequence();
                    printDialog.PrintDocument(fixedDocSeq.DocumentPaginator, header);
                }
            }

            File.Delete(documentPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="header"></param>
        /// <param name="typeface"></param>
        /// <param name="emSize"></param>
        /// <param name="Factory"></param>
        /// <param name="CompoundDocument"></param>
        public static void PrintPreviewDocument(FlowDocument document, string header, Typeface typeface, double emSize, EnhancedUserControlFactory Factory, CompoundDocument CompoundDocument)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            string documentPath = Print.ConvertToXPSDocument(document, header, typeface, emSize, Factory, CompoundDocument);

            PrintPreviewWindow printPreviewWindow = new PrintPreviewWindow(documentPath)
            {
                Owner = Application.Current.MainWindow
            };

            Mouse.OverrideCursor = null;
            printPreviewWindow.ShowDialog();

            File.Delete(documentPath);
        }
    }
}

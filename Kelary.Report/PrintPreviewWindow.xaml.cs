using System.Windows;
using System.Windows.Xps.Packaging;

namespace Kelary.Report
{
	/// <summary>
	/// Interaction logic for PrintPreviewWindow.xaml
	/// </summary>
	public partial class PrintPreviewWindow : Window
	{
		XpsDocument xpsDocument;

		public PrintPreviewWindow(string filePath)
		{
			InitializeComponent();

			this.Closing += new System.ComponentModel.CancelEventHandler(PrintPreviewWindow_Closing);

			xpsDocument = new XpsDocument(filePath, System.IO.FileAccess.Read);

			documentViewer.Document = xpsDocument.GetFixedDocumentSequence();
			documentViewer.FitToMaxPagesAcross(2);
		}

		void PrintPreviewWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			xpsDocument.Close();
		}

	}
}

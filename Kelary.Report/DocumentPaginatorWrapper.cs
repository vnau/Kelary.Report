using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Kelary.Report
{
    /// <summary>
    /// Based on https://docs.microsoft.com/en-us/archive/blogs/fyuan/convert-xaml-flow-document-to-xps-with-style-multiple-page-page-size-header-margin
    /// </summary>
    public class DocumentPaginatorWrapper : DocumentPaginator
    {
        private Size pageSize;
        private Size margin;
        private DocumentPaginator paginator;
        private string header;
        private Typeface typeface;
        private double emSize;

        public DocumentPaginatorWrapper(DocumentPaginator paginator, Size pageSize, Size margin, string header, Typeface typeface, double emSize)
        {
            this.pageSize = pageSize;
            this.margin = margin;
            this.paginator = paginator;
            this.header = header;
            this.typeface = typeface;
            this.emSize = emSize;

            paginator.PageSize = new Size(pageSize.Width - margin.Width * 2,
                                          pageSize.Height - margin.Height * 2);
        }

        private Rect Move(Rect rect)
        {
            if (rect.IsEmpty)
            {
                return rect;
            }
            else
            {
                return new Rect(rect.Left + margin.Width, rect.Top + margin.Height,
                                rect.Width, rect.Height);
            }
        }

        public override System.Windows.Documents.DocumentPage GetPage(int pageNumber)
        {
            System.Windows.Documents.DocumentPage page = paginator.GetPage(pageNumber);

            // Create a wrapper visual for transformation and add extras
            ContainerVisual newpage = new ContainerVisual();

            DrawingVisual title = new DrawingVisual();

            using (DrawingContext ctx = title.RenderOpen())
            {
                #region Page header

                FormattedText text = new FormattedText(header,
                    System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    typeface, emSize, Brushes.Black);

                ctx.DrawText(text, new Point((pageSize.Width - margin.Width * 2 - text.Width) / 2, (-96 / 4))); // 1/4" above page content

                #endregion

                #region Page number

                text = new FormattedText(string.Format("- {0} -", pageNumber + 1),
                    System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    typeface, emSize, Brushes.Black);

                ctx.DrawText(text, new Point((pageSize.Width - margin.Width * 2 - text.Width) / 2, (pageSize.Height - text.Height - margin.Height * 2) + 96 / 4)); // 1/4 inch below page content

                #endregion
            }

            ContainerVisual smallerPage = new ContainerVisual();
            smallerPage.Children.Add(page.Visual);
            smallerPage.Transform = new MatrixTransform(0.95, 0, 0, 0.95, 0.025 * page.ContentBox.Width, 0.025 * page.ContentBox.Height);

            newpage.Children.Add(smallerPage);
            newpage.Children.Add(title);

            newpage.Transform = new TranslateTransform(margin.Width, margin.Height);

            return new System.Windows.Documents.DocumentPage(newpage, pageSize, Move(page.BleedBox), Move(page.ContentBox));
        }

        public override bool IsPageCountValid
        {
            get { return paginator.IsPageCountValid; }
        }

        public override int PageCount
        {
            get { return paginator.PageCount; }
        }

        public override Size PageSize
        {
            get { return paginator.PageSize; }
            set { paginator.PageSize = value; }
        }

        public override IDocumentPaginatorSource Source
        {
            get { return paginator.Source; }
        }
    }
}

namespace Kelary.Report
{
    /// <summary>
    /// 
    /// </summary>
    /// Singleton
    public class CompoundDocumentFactory
    {
        protected CompoundDocumentFactory()
        {
            _CompoundDocument = new CompoundDocument();
        }

        CompoundDocument _CompoundDocument;

        public CompoundDocument CompoundDocument
        {
            get
            {
                return _CompoundDocument;
            }
        }

        private static CompoundDocumentFactory instance;

        /// <summary>
        /// Get reference to instance.
        /// </summary>
        public static CompoundDocumentFactory Instance
        {
            get
            {
                return instance ?? (instance = new CompoundDocumentFactory());
            }
        }

        //public static CompoundDocument CompoundDocument { get; set; }
    }
}

using System;

namespace Kelary.Report
{
    public interface IEnhancedUserControl
    {
        //void Save(IEnhancedUserControl enhancedUserControl, string name, ICloneable dataToPersist);

        void Load(ICloneable dataToPersist);
        ICloneable GetState();

        string GetPrintMarkup();

        string Name { get; set; }

        /// <summary>
        /// Height of the corresponding control.
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// Height of the corresponding control.
        /// </summary>
        double Height { get; set; }
    }
}

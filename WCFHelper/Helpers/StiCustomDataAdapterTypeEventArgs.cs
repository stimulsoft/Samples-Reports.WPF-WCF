using System;
using System.Data.Common;

namespace WCFHelper.Helpers
{
    public delegate void StiCustomDataAdapterTypeEventHandlers(object sender, StiCustomDataAdapterTypeEventArgs e);

    public class StiCustomDataAdapterTypeEventArgs : EventArgs
    {
        public StiCustomDataAdapterTypeEventArgs(string typeName)
        {
            this.TypeName = typeName;
        }

        #region Properties
        public string TypeName { get; }

        public DbConnection Connection { get; set; }
        #endregion
    }
}
using System;
using Stimulsoft.Report;

namespace WCFHelper
{
    internal class StiDataBandSelectionContainer
    {
        public StiReport Report;
        public string[] DataBandNames;
        public int[] SelectedLines;

        public StiDataBandSelectionContainer()
        {
            this.Report = new StiReport();
        }
    }
}
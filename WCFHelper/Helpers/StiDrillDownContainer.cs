using System;
using System.Collections;
using System.Collections.Generic;

using Stimulsoft.Report;
using Stimulsoft.Report.Components;

namespace WCFHelper
{
    internal class StiDrillDownContainer
    {
        #region Fields
        public string TypeDrillDown;

        public string DrillDownPageName;
        public Dictionary<string, object> DrillDownParameters;
        public int PageIndex;
        public int CompIndex;
        public StiReport Report;

        public string DataBandName;
        public string[] DataBandColumns;
        public string DataBandColumnString;
        public StiInteractionSortDirection SortingDirection;
        public bool IsControlPress;
        public int CollapsingIndex;
        public bool IsCollapsed;
        public Hashtable InteractionCollapsingStates;
        #endregion

        public StiDrillDownContainer()
        {
            TypeDrillDown = string.Empty;

            DrillDownPageName = string.Empty;
            DrillDownParameters = new Dictionary<string, object>();
            Report = new StiReport();

            DataBandName = string.Empty;
            DataBandColumnString = string.Empty;
            SortingDirection = StiInteractionSortDirection.None;
        }
    }
}
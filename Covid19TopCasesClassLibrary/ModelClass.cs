using System;
using System.Collections.Generic;

namespace Covid19TopCasesClassLibrary
{
    #region Regions catalog
    /// <summary>
    /// List of regions
    /// </summary>
    public class RegionList
    {
        public List<Region> Data { get; set; }
    }

    /// <summary>
    /// Region object
    /// </summary>
    public class Region
    {
        public string Iso { get; set; }
        public string Name { get; set; }
    }
    #endregion
}

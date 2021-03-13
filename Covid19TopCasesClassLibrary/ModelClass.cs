﻿using System;
using System.Collections.Generic;

namespace Covid19TopCasesClassLibrary
{
    #region Regions catalog from RapidAPI
    /// <summary>
    /// List of regions
    /// </summary>
    public class RegionCatalog
    {
        public List<RegionObject> Data { get; set; }
    }

    /// <summary>
    /// Region object
    /// </summary>
    public class RegionObject
    {
        public string Iso { get; set; }
        public string Name { get; set; }
    }
    #endregion

    #region Provinces catalog from RapidAPI
    /// <summary>
    /// List of provinces
    /// </summary>
    public class ProvinceCatalog
    {
        public List<ProvinceObject> Data { get; set; }
    }

    /// <summary>
    /// Province object
    /// </summary>
    public class ProvinceObject
    {
        public string Iso { get; set; }
        public string Name { get; set; }
        public string Province { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }
    }
    #endregion

    #region Global reports from RapidAPI
    /// <summary>
    /// 
    /// </summary>
    public class GlobalReport
    {
        public List<GlobalData> Data { get; set; }
    }

    public class GlobalData
    {
        public DateTime Date { get; set; }
        public long Confirmed { get; set; }
        public long Deaths { get; set; }
        public long Recovered { get; set; }
        public long ConfirmedDiff { get; set; }
        public long DeathsDiff { get; set; }
        public long RecoveredDiff { get; set; }
        public DateTime LastUpdate { get; set; }
        public long Active { get; set; }
        public long ActiveDiff { get; set; }
        public double FatalityRate { get; set; }
        public Region Region { get; set; }
    }

    public class Region
    {
        public string Iso { get; set; }
        public string Name { get; set; }
        public string Province { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }
        public List<City> Cities { get; set; }
    }

    public class City
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public long? Fips { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }
        public long Confirmed { get; set; }
        public long Deaths { get; set; }
        public long ConfirmedDiff { get; set; }
        public long DeathsDiff { get; set; }
        public DateTime LastUpdate { get; set; }
    }
    #endregion

    #region RESTful API Control Classes
    public class RequestCovid19Stats
    {
        public string RegionIso { get; set; }
    }

    public class TopRegionsStatistics
    {
        public string Region { get; set; }
        public long Cases { get; set; }
        public long Deaths { get; set; }
        public string CasesStr { get; set; }
        public string DeathsStr { get; set; }
    }

    public class TopProvincesStatistics
    {
        public string Province { get; set; }
        public long Cases { get; set; }
        public long Deaths { get; set; }
        public string CasesStr { get; set; }
        public string DeathsStr { get; set; }
    }

    public class ServiceResponse
    {
        public int Status { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public object Data { get; set; }
    }
    #endregion
}

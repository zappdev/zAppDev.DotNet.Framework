// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
using System.Configuration;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration
{
#region Base Classes
    public class Enabled_Attribute : ConfigurationElement
    {
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = false)]
        public bool? Enabled => this["enabled"] as bool?;
    }
#endregion

#region Common Attributes, linked together

    public class MinimumMilliseconds_Enabled_Attribute : Enabled_Attribute
    {
        [ConfigurationProperty("minimumMilliseconds", IsRequired = false)]
        public long? MinimumMilliseconds => this["minimumMilliseconds"] as long?;
    }

    public class IgnoreNulls_Enabled_Attribute : Enabled_Attribute
    {
        [ConfigurationProperty("ignoreNulls", IsRequired = false, DefaultValue = true)]
        public bool? IgnoreNulls => this["ignoreNulls"] as bool?;
    }

    public class MinimumBytes_Enabled_Attribute : Enabled_Attribute
    {
        [ConfigurationProperty("minimumBytes", IsRequired = false)]
        public long? MinimumBytes => this["minimumBytes"] as long?;
    }
#endregion


#region Simple Elements
    public class FrontEndElement: Enabled_Attribute { }
    public class SizeElement : MinimumBytes_Enabled_Attribute { }
    public class TimeElement : MinimumMilliseconds_Enabled_Attribute { }
    public class RAMElement : MinimumBytes_Enabled_Attribute
    {
        [ConfigurationProperty("samplingIntervalMilliseconds", IsRequired = false, DefaultValue = 1000)]
        public int? IntervalMilliseconds => this["samplingIntervalMilliseconds"] as int?;
    }

    public class SessionElement : IgnoreNulls_Enabled_Attribute {
        [ConfigurationProperty("monitoredStatistics", IsRequired = false, DefaultValue = null)]
        public string MonitoredStatistics => this["monitoredStatistics"] as string;
    }

    public class EntitiesElement : IgnoreNulls_Enabled_Attribute {
        [ConfigurationProperty("monitoredStatistics", IsRequired = false, DefaultValue = null)]
        public string MonitoredStatistics => this["monitoredStatistics"] as string;

        [ConfigurationProperty("monitoredEntities", IsRequired = false, DefaultValue = null)]
        public string MonitoredEntities => this["monitoredEntities"] as string;
    }


    public class CPUElement : Enabled_Attribute
    {
        [ConfigurationProperty("minimumPercentage", IsRequired = false)]
        public float? MinimumPercentage => this["minimumPercentage"] as float?;

        [ConfigurationProperty("samplingIntervalMilliseconds", IsRequired = false, DefaultValue = 1000)]
        public int? IntervalMilliseconds => this["samplingIntervalMilliseconds"] as int?;
    }
#endregion

#region Complex Elements
    public class DatabaseElement : Enabled_Attribute
    {
        [ConfigurationProperty("session", IsRequired = false)]
        public SessionElement Session => this["session"] as SessionElement;

        [ConfigurationProperty("entities", IsRequired = false)]
        public EntitiesElement Entities => this["entities"] as EntitiesElement;
    }

    public class ClientDataElement : Enabled_Attribute
    {
        [ConfigurationProperty("size", IsRequired = false)]
        public SizeElement Size => this["size"] as SizeElement;

        [ConfigurationProperty("time", IsRequired = false)]
        public TimeElement Time => this["time"] as TimeElement;
    }

    public class DBFlushElement : Enabled_Attribute
    {
        [ConfigurationProperty("time", IsRequired = false)]
        public TimeElement Time => this["time"] as TimeElement;

        [ConfigurationProperty("database", IsRequired = false)]
        public DatabaseElement Database => this["database"] as DatabaseElement;
    }

    public class ControllerActionElement : Enabled_Attribute
    {
        [ConfigurationProperty("frontEnd", IsRequired = false)]
        public FrontEndElement FrontEnd => this["frontEnd"] as FrontEndElement;

        [ConfigurationProperty("time", IsRequired = false)]
        public TimeElement Time => this["time"] as TimeElement;

        [ConfigurationProperty("cpu", IsRequired = false)]
        public CPUElement CPU => this["cpu"] as CPUElement;

        [ConfigurationProperty("ram", IsRequired = false)]
        public RAMElement RAM => this["ram"] as RAMElement;

        [ConfigurationProperty("database", IsRequired = false)]
        public DatabaseElement Database => this["database"] as DatabaseElement;
    }

    public class ExposedApiElement : Enabled_Attribute
    {
        [ConfigurationProperty("time", IsRequired = false)]
        public TimeElement Time => this["time"] as TimeElement;

        [ConfigurationProperty("cpu", IsRequired = false)]
        public CPUElement CPU => this["cpu"] as CPUElement;

        [ConfigurationProperty("ram", IsRequired = false)]
        public RAMElement RAM => this["ram"] as RAMElement;

        [ConfigurationProperty("database", IsRequired = false)]
        public DatabaseElement Database => this["database"] as DatabaseElement;
    }

    public class DTO2ViewModelElement : Enabled_Attribute
    {
        [ConfigurationProperty("size", IsRequired = false)]
        public SizeElement Size => this["size"] as SizeElement;

        [ConfigurationProperty("time", IsRequired = false)]
        public TimeElement Time => this["time"] as TimeElement;

        [ConfigurationProperty("database", IsRequired = false)]
        public DatabaseElement Database => this["database"] as DatabaseElement;
    }

    public class Model2DTOElement : Enabled_Attribute
    {
        [ConfigurationProperty("size", IsRequired = false)]
        public SizeElement Size => this["size"] as SizeElement;

        [ConfigurationProperty("time", IsRequired = false)]
        public TimeElement Time => this["time"] as TimeElement;
    }

    public class ConversionsElement : Enabled_Attribute
    {
        [ConfigurationProperty("DTO2ViewModel", IsRequired = false)]
        public DTO2ViewModelElement DTO2ViewModel => this["DTO2ViewModel"] as DTO2ViewModelElement;

        [ConfigurationProperty("Model2DTO", IsRequired = false)]
        public Model2DTOElement Model2DTO => this["Model2DTO"] as Model2DTOElement;
    }

    public class DataElement: Enabled_Attribute
    {
        [ConfigurationProperty("clientData", IsRequired = false)]
        public ClientDataElement ClientData => this["clientData"] as ClientDataElement;

        [ConfigurationProperty("DTO2ViewModel", IsRequired = false)]
        public DTO2ViewModelElement DTO2ViewModel => this["DTO2ViewModel"] as DTO2ViewModelElement;

        [ConfigurationProperty("Model2DTO", IsRequired = false)]
        public Model2DTOElement Model2DTO => this["Model2DTO"] as Model2DTOElement;
    }

#endregion

    public class PerformanceMonitorConfigurationReader : ConfigurationSection
    {
        public const string SectionName = "performanceMonitor";

        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = false)]
        public bool? Enabled => this["enabled"] as bool?;

        [ConfigurationProperty("controllerAction", IsRequired = false)]
        public ControllerActionElement ControllerAction => this["controllerAction"] as ControllerActionElement;

        [ConfigurationProperty("exposedApi", IsRequired = false)]
        public ExposedApiElement ExposedApi => this["exposedApi"] as ExposedApiElement;

        [ConfigurationProperty("data", IsRequired = false)]
        public DataElement DataElement => this["data"] as DataElement;

        [ConfigurationProperty("dbFlush", IsRequired = false)]
        public DBFlushElement DBFlush => this["dbFlush"] as DBFlushElement;
    }

}


#else


namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration
{
    #region Base Classes
    public class Enabled_Attribute
    {
        public bool? Enabled { get; set; }
    }
    #endregion

    #region Common Attributes, linked together

    public class MinimumMilliseconds_Enabled_Attribute : Enabled_Attribute
    {
        public long? MinimumMilliseconds { get; set; }
    }

    public class IgnoreNulls_Enabled_Attribute : Enabled_Attribute
    {
        public bool? IgnoreNulls { get; set; }
    }

    public class MinimumBytes_Enabled_Attribute : Enabled_Attribute
    {
        public long? MinimumBytes { get; set; }
    }
    #endregion


    #region Simple Elements
    public class FrontEndElement : Enabled_Attribute { }
    public class SizeElement : MinimumBytes_Enabled_Attribute { }
    public class TimeElement : MinimumMilliseconds_Enabled_Attribute { }
    public class RAMElement : MinimumBytes_Enabled_Attribute
    {
        public int? IntervalMilliseconds { get; set; }
    }

    public class SessionElement : IgnoreNulls_Enabled_Attribute {
        public string MonitoredStatistics { get; set; }
    }

    public class EntitiesElement : IgnoreNulls_Enabled_Attribute {
        public string MonitoredStatistics { get; set; }

        public string MonitoredEntities { get; set; }
    }


    public class CPUElement : Enabled_Attribute
    {
        public float? MinimumPercentage { get; set; }

        public int? IntervalMilliseconds { get; set; }
    }
    #endregion

    #region Complex Elements
    public class DatabaseElement : Enabled_Attribute
    {
        public SessionElement Session { get; set; }

        public EntitiesElement Entities { get; set; }
    }

    public class ClientDataElement : Enabled_Attribute
    {
        public SizeElement Size { get; set; }

        public TimeElement Time { get; set; }
    }

    public class DBFlushElement : Enabled_Attribute
    {
        public TimeElement Time { get; set; }

        public DatabaseElement Database { get; set; }
    }

    public class ControllerActionElement : Enabled_Attribute
    {
        public FrontEndElement FrontEnd { get; set; }

        public TimeElement Time { get; set; }

        public CPUElement CPU { get; set; }

        public RAMElement RAM { get; set; }

        public DatabaseElement Database { get; set; }
    }

    public class ExposedApiElement : Enabled_Attribute
    {
        public TimeElement Time { get; set; }

        public CPUElement CPU { get; set; }

        public RAMElement RAM { get; set; }

        public DatabaseElement Database { get; set; }
    }

    public class DTO2ViewModelElement : Enabled_Attribute
    {
        public SizeElement Size { get; set; }

        public TimeElement Time { get; set; }

        public DatabaseElement Database { get; set; }
    }

    public class Model2DTOElement : Enabled_Attribute
    {
        public SizeElement Size { get; set; }

        public TimeElement Time { get; set; }
    }

    public class ConversionsElement : Enabled_Attribute
    {
        public DTO2ViewModelElement DTO2ViewModel { get; set; }

        public Model2DTOElement Model2DTO { get; set; }
    }

    public class DataElement : Enabled_Attribute
    {
        public ClientDataElement ClientData { get; set; }

        public DTO2ViewModelElement DTO2ViewModel { get; set; }

        public Model2DTOElement Model2DTO { get; set; }
    }

    #endregion

    public class PerformanceMonitorConfigurationReader
    {
        public const string SectionName = "performanceMonitor";

        public bool? Enabled { get; set; }

        public ControllerActionElement ControllerAction { get; set; }

        public ExposedApiElement ExposedApi { get; set; }

        public DataElement DataElement { get; set; }

        public DBFlushElement DBFlush { get; set; }
    }

}

#endif
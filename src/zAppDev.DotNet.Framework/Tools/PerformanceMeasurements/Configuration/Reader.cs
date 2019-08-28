#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [ConfigurationProperty("data", IsRequired = false)]
        public DataElement DataElement => this["data"] as DataElement;

        [ConfigurationProperty("dbFlush", IsRequired = false)]
        public DBFlushElement DBFlush => this["dbFlush"] as DBFlushElement;
    }

}


#endif
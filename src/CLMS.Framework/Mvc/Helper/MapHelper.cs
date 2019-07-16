using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;

namespace CLMS.Framework.Mvc
{
    public class MapHelper
    {
        public List<MapHelperPoint> Points;
        public List<MapHelperLine> Lines;

        public GeoJsonObject ToGeoJson()
        {
            var root = new GeoJsonFeatureCollection();

            if (Points != null)
                foreach (var point in Points)
                {
                    if (point.Coordinates?.Longitude == null || point.Coordinates?.Latitude == null) continue;

                    var geoJsonFeature = new GeoJsonFeature();
                    var geoJsonPoint = new GeoJsonGeometry();
                    dynamic properties = new ExpandoObject();

                    geoJsonPoint.GeoJsonObjectType = "Point";
                    geoJsonPoint.Coordinates = new[] { point?.Coordinates?.Longitude, point?.Coordinates?.Latitude };

                    properties.style = new ExpandoObject();
                    if (!string.IsNullOrWhiteSpace(point.Icon)) properties.style.icon = point.Icon;
                    if (!string.IsNullOrWhiteSpace(point.Label)) properties.style.label = point.Label;
                    if (!string.IsNullOrWhiteSpace(point.Tooltip)) properties.style.title = point.Tooltip;
                    if (!string.IsNullOrWhiteSpace(point.Animation)) properties.style.animation = point.Animation;

                    properties.boundObject = point.BoundObject;
                    geoJsonFeature.Properties = properties;
                    geoJsonFeature.Geometry = geoJsonPoint;

                    root.Features.Add(geoJsonFeature);
                }

            if (Lines != null)
                foreach (var line in Lines)
                {
                    if (line?.Origin?.Longitude == null || line?.Origin?.Latitude == null ||
                        line.Destination.Longitude == null || line.Destination.Latitude == null) continue;

                    var geoJsonFeature = new GeoJsonFeature();
                    var geoJsonLine = new GeoJsonGeometry();

                    dynamic properties = new ExpandoObject();

                    geoJsonLine.GeoJsonObjectType = "LineString";
                    geoJsonLine.Coordinates = new[]
                    {
                        new[] { line.Origin.Longitude, line.Origin.Latitude },
                        new[] { line.Destination.Longitude, line.Destination.Latitude }
                    };

                    properties.style = new ExpandoObject();

                    if (line.IsCurved != null) properties.style.geodesic = line.IsCurved;
                    if (!string.IsNullOrWhiteSpace(line.StrokeColor)) properties.style.strokeColor = line.StrokeColor;
                    if (line.StrokeWidth != null) properties.style.strokeWeight = line.StrokeWidth;
                    if (line.StrokeOpacity != null) properties.style.strokeOpacity = line.StrokeOpacity;
                    if (line.IsArrow != null) properties.style.arrowSymbol = true;

                    properties.boundObject = line.BoundObject;

                    geoJsonFeature.Properties = properties;
                    geoJsonFeature.Geometry = geoJsonLine;

                    root.Features.Add(geoJsonFeature);
                }

            return root;
        }
    }

    public class MapHelperLine
    {
        public decimal? StrokeWidth { get; set; }
        public decimal? StrokeOpacity { get; set; }

        public string StrokeColor { get; set; }
        public bool? IsArrow { get; set; }

        public bool? IsCurved { get; set; }

        public MapHelperCoordinates Origin { get; set; }
        public MapHelperCoordinates Destination { get; set; }

        public object BoundObject { get; set; }
    }

    public class MapHelperPoint
    {
        public MapHelperCoordinates Coordinates { get; set; }
        public string Icon { get; set; }
        public string Tooltip { get; set; }
        public string Label { get; set; }
        public string Animation { get; set; }
        public object BoundObject { get; set; }
    }

    public class MapHelperCoordinates
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class GeoJsonObject
    {
        /*
         * Valid types: Point,
         *              MultiPoint, LineString,
         *              MultiLineString,
         *              Polygon,
         *              MultiPolygon,
         *              GeometryCollection,
         *              Feature,
         *              FeatureCollection         
         */
        [JsonProperty("type")]
        public string GeoJsonObjectType { get; set; }
        [JsonProperty("crs")]
        public GeoJsonCoordinateReferenceSystem CoordinateReferenceSystem { get; set; }
        [JsonProperty("bbox")]
        public List<object> BoundingBox { get; set; }
    }

    /*
     * NOTE: Provide coordinates to the appropriate system according to the end-point that uses these classes
     * 
     * Google Earth is in a Geographic coordinate system with the wgs84 datum. (EPSG: 4326)
     * Google Maps is in a projected coordinate system that is based on the wgs84 datum. (EPSG 3857)
     * The data in Open Street Map database is stored in a gcs with units decimal degrees & datum of wgs84. (EPSG: 4326)
     * The Open Street Map tiles and the WMS webservice, are in the projected coordinate system that is based on the wgs84 datum. (EPSG 3857)
     * 
     */

    public class GeoJsonCoordinateReferenceSystem
    {
        /*
         * Valid types: Named
         *              Linked
         */
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("properties")]
        public object Properties { get; set; }
    }

    public class GeoJsonFeature : GeoJsonObject
    {
        [JsonProperty("properties")]
        public object Properties { get; set; }
        [JsonProperty("geometry")]
        public GeoJsonGeometry Geometry { get; set; }
        //NOTE ommited geometry collection
        public GeoJsonFeature()
        {
            GeoJsonObjectType = "Feature";
        }
    }

    public class GeoJsonFeatureCollection : GeoJsonObject
    {
        [JsonProperty("features")]
        public List<GeoJsonFeature> Features { get; set; }

        public GeoJsonFeatureCollection()
        {
            GeoJsonObjectType = "FeatureCollection";
            Features = new List<GeoJsonFeature>();
        }
    }

    /*
    * NOTE: Coordinates are stored as x, y, z in the following order:
    * 
    * - Easting, Northing, Altitude for coordinates in a projected coordinate reference system
    * - Longitude, Latitude, Altitude for coordinates in a geographic coordinate reference system
    */

    public class GeoJsonGeometry : GeoJsonObject
    {
        [JsonProperty("coordinates")]
        public object Coordinates { get; set; }
    }
}
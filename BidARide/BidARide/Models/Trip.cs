using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace BidARide.Models
{

    public class LocationInfo
    {
        public string address { get; set; }
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class DirectionInfo
    {
        public string distance_text { get; set; }
        public float distance_value { get; set; }
        public LocationInfo start { get; set; }
        public LocationInfo end { get; set; }

        public DirectionInfo()
        {
            start = new LocationInfo();
            end = new LocationInfo();
        }

        public DirectionInfo(string json)
        {
            var jsonObject = Json.Decode(json);
            start.address = jsonObject.status;
            //geocoded_waypoints = (DynamicJsonArray)jsonObject.geocoded_waypoints;
            //routes = (DynamicJsonArray)jsonObject.routes;
        }
    }


    public class Trip
    {
        public string userID { get; set; }
        public string driverID { get; set; }

        [Required]
        [Display(Name = "From...")]
        public string tripFrom { get; set; }

        [Required]
        [Display(Name = "To...")]
        public string tripTo { get; set; }

        public float fromLong { get; set; }
        public float fromLat { get; set; }
        public float toLong { get; set; }
        public float toLat { get; set; } 
    }

    public class JsonDirectionInfo
    {
        public DynamicJsonArray geocoded_waypoints { get; set; }
        public DynamicJsonArray routes { get; set; }
        public string status { get; set; }

        public JsonDirectionInfo(string json)
        {
            var jsonObject = Json.Decode(json);
            status = jsonObject.status;
            geocoded_waypoints = (DynamicJsonArray)jsonObject.geocoded_waypoints;
            routes = (DynamicJsonArray)jsonObject.routes;
        }
    }
}
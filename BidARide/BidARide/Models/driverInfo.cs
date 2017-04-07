using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace BidARide.Models
{
    public class DriverInfomation
    {
        public string driverID { get; set; }
        public string driverFullname { get; set; }
        public string driverPhone { get; set; }
        public float longtitude { get; set; }
        public float latitude { get; set; }

        public DriverInfomation() { }
        public DriverInfomation(string id, string fullname, string phone, float lng, float lat)
        {
            driverID = id;
            driverFullname = fullname;
            driverPhone = phone;
            longtitude = lng;
            latitude = lat;
        }
    }
    public class JsonInfo
    {
        public int status { get; set; }
        public DynamicJsonArray payload { get; set; }

        public JsonInfo(string json)
        {
            var jsonObject = Json.Decode(json);
            status = (int)jsonObject.status;
            payload = (DynamicJsonArray)jsonObject.payload;
        }

        
    }
}
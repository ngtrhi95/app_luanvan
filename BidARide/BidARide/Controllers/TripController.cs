using BidARide.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace BidARide.Controllers
{
    public class TripController : Controller
    {
        const string API_Key = "AIzaSyBIAO62sl7gODaykvAl9VjDig8DLeqvThA";
        LocationInfo From, To;
        List<DriverInfomation> ListDriverNearLocation;
        //
        // GET: /Home/
        public ActionResult Index()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Session.Remove("direction");
            Session.Remove("From");
            Session.Remove("To");
            return View();
        }

        public JsonResult GetNearDriver()
        {
            ListDriverNearLocation = getNearDiver();
            if (ListDriverNearLocation == null || ListDriverNearLocation.Count == 0)
            {
                ViewBag.Error = "No driver near your location.";
            }
            return new JsonResult { Data = ListDriverNearLocation, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetDirection()
        {
            LocationInfo from = new LocationInfo();
            from.address = Session["From"].ToString();
            LocationInfo to = new LocationInfo();
            to.address = Session["To"].ToString();

            DirectionInfo v = getDirection(from, to);
            Session["direction"] = v;
            return new JsonResult { Data = v, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private List<DriverInfomation> getAllDriver()
        {
            var request = WebRequest.Create("https://fast-hollows-58498.herokuapp.com/driver/coordInfo");
            request.ContentType = "application/json; charset=utf-8";
            string text;
            var response = (HttpWebResponse)request.GetResponse();

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd();

                JavaScriptSerializer js = new JavaScriptSerializer();

                JsonInfo jsoninfo = new JsonInfo(text);
                if (jsoninfo.status != 200)
                {
                    @ViewBag.Error = "Error " + jsoninfo.status;
                    return null;
                }

                if (jsoninfo.payload.Length == 0)
                {
                    @ViewBag.Error = "No driver near your location.";
                    return null;
                }

                // Cắt chuỗi để lấy thông tin JSon Payload
                string JsonPayload = text.Remove(0, 24);
                JsonPayload = JsonPayload.Remove(JsonPayload.Length - 1);

                dynamic dynObj = JsonConvert.DeserializeObject(JsonPayload);
                List<DriverInfomation> listDriver = new List<DriverInfomation>();
                foreach (var item in dynObj)
                {
                    string id = item.driverID;
                    string fullname = item.driverFullname;
                    string phone = item.driverPhone;
                    float lng = item.longitude;
                    float lat = item.latitude;
                    DriverInfomation driver = new DriverInfomation(id, fullname, phone, lng, lat);
                    listDriver.Add(driver);
                }

                return listDriver;
            }
        }

        private DirectionInfo getDirection(LocationInfo from, LocationInfo to)
        {
            DirectionInfo direction = new DirectionInfo();
            string origin = from.address, destination = to.address;
            if (origin == "" || origin == null)
            {
                origin = from.lat + "," + from.lng;
            }
            
            if (destination == "" || destination == null)
            {
                destination = to.lat + "," + to.lng;
            }

            var request = WebRequest.Create("https://maps.googleapis.com/maps/api/directions/json?origin=" + origin + "&destination=" + destination + "&key=" + API_Key);
            request.ContentType = "application/json; charset=utf-8";
            string text;
            var response = (HttpWebResponse)request.GetResponse();

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd();

                JavaScriptSerializer js = new JavaScriptSerializer();

                JsonDirectionInfo jsoninfo = new JsonDirectionInfo(text);
                if (jsoninfo.status != "OK")
                {
                    ViewBag.Error = "Error " + jsoninfo.status;
                    return null;
                }

                if (jsoninfo.routes.Length == 0)
                {
                    ViewBag.Error = "No direction.";
                    return null;
                }
                //string userID = Session["username"].ToString();
                dynamic d = JObject.Parse(text);
                var directions_legs = d.routes[0].legs;

                direction.distance_text = directions_legs[0].distance.text;
                direction.distance_value = directions_legs[0].distance.value;

                direction.start.address = directions_legs[0].start_address;
                direction.start.lat = directions_legs[0].start_location.lat;
                direction.start.lng = directions_legs[0].start_location.lng;

                direction.end.lat = directions_legs[0].end_location.lat;
                direction.end.lng = directions_legs[0].end_location.lng;
                direction.end.address = directions_legs[0].end_address;
            }
            return direction;
        }

        private List<DriverInfomation> getNearDiver()
        {
            LocationInfo from = new LocationInfo();
            from.address = Session["From"].ToString();
            ListDriverNearLocation = new List<DriverInfomation>();
            List<DriverInfomation> lstAllDriver = getAllDriver();
            foreach (DriverInfomation driver in lstAllDriver)
            {
                LocationInfo lc_diver = new LocationInfo();
                lc_diver.lat = driver.latitude;
                lc_diver.lng = driver.longtitude;
                DirectionInfo direc = getDirection(lc_diver, from);
                if (direc.distance_value <= 5000)
                {
                    ListDriverNearLocation.Add(driver);
                }
            }

            return ListDriverNearLocation;
        }

        public ActionResult findDriver (Trip model)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ListDriverNearLocation = new List<DriverInfomation>();

            //From = getLocationFromAddress(model.tripFrom);
            //To = getLocationFromAddress(model.tripTo);
            LocationInfo from = new LocationInfo(), to = new LocationInfo();
            from.address = model.tripFrom; to.address = model.tripTo;
            Session["From"] = model.tripFrom;
            Session["To"] = model.tripTo;
            DirectionInfo direction = getDirection(from, to);

            
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Booking(DriverInfomation driver)
        {
            HttpClient client = new HttpClient();
            string userID = Session["userID"].ToString();
            dynamic dir_session = Session["direction"];

            var values = new Dictionary<string, string>
            {
                { "userID", userID },
                { "driverID", driver.driverID },
                { "tripFrom", dir_session.start.address },
                { "tripTo", dir_session.end.address },
                { "fromLong", dir_session.start.lng.ToString()},
                { "fromLat", dir_session.start.lat.ToString() },
                { "toLong", dir_session.end.lng.ToString() },
                { "toLat", dir_session.end.lat.ToString() },

            };
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://fast-hollows-58498.herokuapp.com/trip/create", content);
            var responseString = await response.Content.ReadAsStringAsync();

            string s = responseString.ToString();
            if (s != "OK")
            {
                ViewBag.Error = "Username or Email already exist";

            }
                //Session["username"] = model.account.Username;
            return new JsonResult { Data = "Sucessfull", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        //[HttpGet]
        //[AllowAnonymous]
        //private LocationInfo getLocationFromAddress(string address)
        //{
        //    LocationInfo location = new LocationInfo();
            
        //    var request = WebRequest.Create("https://maps.googleapis.com/maps/api/geocode/json?address=" + address + "&key=" + API_Key);
        //    request.ContentType = "application/json; charset=utf-8";
        //    string text;
        //    var response = (HttpWebResponse)request.GetResponse();

        //    using (var sr = new StreamReader(response.GetResponseStream()))
        //    {
        //        text = sr.ReadToEnd();

        //        JavaScriptSerializer js = new JavaScriptSerializer();

        //        JsonLocationInfo jsoninfo = new JsonLocationInfo(text);
        //        if (jsoninfo.status != "OK")
        //        {
        //            @ViewBag.Error = "Error " + jsoninfo.status;
        //            return null;
        //        }

        //        if (jsoninfo.results.Length == 0)
        //        {
        //            @ViewBag.Error = "No driver near your location.";
        //            return null;
        //        }
        //        string userID = Session["username"].ToString();

        //        string JsonResult = text.Remove(text.Length - 23, 23).Trim();
        //        JsonResult = JsonResult.Remove(0, 16);
        //        dynamic dynObj = JsonConvert.DeserializeObject(JsonResult);
        //        //List<DriverInfomation> listDriver = new List<DriverInfomation>();
        //        foreach (var item in dynObj)
        //        {
        //            location.address = item.formatted_address;
        //            location.lng = item.geometry.location.lng;
        //            location.lat = item.geometry.location.lat;
        //        }
        //    }

        //    return location;
        //}
       }
}

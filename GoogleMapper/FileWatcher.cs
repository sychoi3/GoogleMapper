using GoogleMapper.Hubs;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;


namespace GoogleMapper
{

    public class Coordinate
    {
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class FileWatcher
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("LOG");

        private static FileSystemWatcher fWatcher;
        private static String xmlFilePath = HttpContext.Current.Server.MapPath("~/App_Data/location.xml");
        private static List<Coordinate> objLoc = new List<Coordinate>();

        public static IEnumerable<Coordinate> Data
        {
            get
            {
                return objLoc.AsReadOnly();
            }
        }

        /// <summary>
        /// One Time Initialization
        /// </summary>
        public static void init()
        {
            Initialize();
            ReadNewLines();
        }

        /// <summary>
        /// To setup FileSystemWatcher
        /// </summary>
        private static void Initialize()
        {
            log.Info("initializing FileSystemWatcher...");
            fWatcher = new FileSystemWatcher();

            fWatcher.Path = Path.GetDirectoryName(xmlFilePath);
            fWatcher.Filter = Path.GetFileName(xmlFilePath);

            fWatcher.Changed += watcher_Changed;
            fWatcher.EnableRaisingEvents = true;
            fWatcher.Error += OnError;
        }

        /// <summary>
        /// Error Handling
        /// </summary>
        private static void OnError(object sender, ErrorEventArgs e)
        {
            fWatcher.Dispose();
            Initialize();
        }

        /// <summary>
        /// File Changed Event Handler
        /// </summary>
        private static void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                log.Info("change detected!");
                fWatcher.EnableRaisingEvents = false;   //There is a bug in FileSystemWatcher (it fires the event twice).  To Fix it, turn off, execute, turn on <-- this will make sure it fires once.
                var newLoc = ReadNewLines();
                //Pass new points to client
                var context = GlobalHost.ConnectionManager.GetHubContext<MapsHub>();
                context.Clients.All.addLocations(newLoc);
            }
            finally
            {
                fWatcher.EnableRaisingEvents = true;
            }

        }

        /// <summary>
        /// To Read New Lines 
        /// </summary>
        public static IEnumerable<Coordinate> ReadNewLines()
        {
            XDocument xmlDoc;
            using (Stream s = File.OpenRead(xmlFilePath))
            {
                xmlDoc = XDocument.Load(s);
            }
            int total = objLoc.Count();
            var newLoc = (from item in xmlDoc.Descendants("location").Select((x, index) => new { x, index })
                          where item.index >= total
                          select new Coordinate
                          {
                              Name = (String)item.x.Attribute("name"),
                              Lat = (double)item.x.Attribute("lat"),
                              Lng = (double)item.x.Attribute("lng")
                          });
            objLoc.AddRange(newLoc);
            log.Info("Locations Added: " + newLoc.Count());
            log.Info("NEW LOCATIONS: ");
            foreach(var item in newLoc)
            {
                log.Info(string.Format("  _LocationName: {0} Latitude: {1} Longitude: {2}", item.Name, item.Lat, item.Lng));
            }

            return newLoc;
        }
    }
}
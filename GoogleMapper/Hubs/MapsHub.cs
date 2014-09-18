using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace GoogleMapper.Hubs
{
    public class MapsHub : Hub
    {
        public IEnumerable<Coordinate> GetLocations()
        {
            return FileWatcher.Data;
        }
    }
}
GoogleMapperTest
================

Google Maps For Real-Time Location Updates

Web application monitors an external XML file that can potentially be written by external processes and updates the client side browser with Real-time location information.

Web application uses FileSystemWatcher to monitor an XML file and fires an event from the server to the client via SignalR when it detects a change. The server side extracts information from XML and sends Latitude and Longitude information to the client side. The client then uses Google Maps API to display the locations in the form of an animated circle. Log4Net Logger has been implemented to provide logging.

using System;
using CoreLocation;
using UIKit;

namespace PitStop.iPhone
{
	public class LocationManager
	{
		protected CLLocationManager locMgr;

		// event for the location changing
		public event EventHandler<LocationUpdatedEventArgs> LocationUpdated = delegate { };

		public LocationManager ()
		{
			this.locMgr = new CLLocationManager();
			// iOS 8 has additional permissions requirements
			//
			if (UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) {
				locMgr.RequestAlwaysAuthorization (); // works in background
				//locMgr.RequestWhenInUseAuthorization (); // only in foreground
			}
		}

		public void StartLocationUpdates()
		{
			if (CLLocationManager.LocationServicesEnabled) {
				//set the desired accuracy, in meters
				LocMgr.DesiredAccuracy = 1;
				LocMgr.LocationsUpdated += (object sender, CLLocationsUpdatedEventArgs e) =>
				{
					// fire our custom Location Updated event
					LocationUpdated (this, new LocationUpdatedEventArgs (e.Locations [e.Locations.Length - 1]));
				};
				LocMgr.StartUpdatingLocation();
			}
		}

		public CLLocationManager LocMgr{
			get { return this.locMgr; }
		}
	}
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using System.Threading.Tasks;

namespace PitStop.Droid
{
	[Activity (Label = "GetLocation")]			
	public class GetLocation : Activity, ILocationListener
	{
		Location _currentLocation;
		LocationManager _locationManager;
		TextView _locationText;
		TextView _addressText;
		String _locationProvider;

		public List<Polyline> polyLines;
		public TravelData originalRoute;

		List<Marker> markers;
		int index = 0;

		public void OnLocationChanged(Location location)
		{
			_currentLocation = location;
			if (_currentLocation == null)
			{
				_locationText.Text = "Unable to determine your location.";
			}
			else
			{
				_locationText.Text = String.Format("{0},{1}", _currentLocation.Latitude, _currentLocation.Longitude);
			}
			markers [markers.Count-1].Position = new LatLng(_currentLocation.Latitude, _currentLocation.Longitude);
		}

		public void OnProviderDisabled(string provider) {}

		public void OnProviderEnabled(string provider) {}

		public void OnStatusChanged(string provider, Availability status, Bundle extras) {}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);

			_addressText = FindViewById<TextView>(Resource.Id.address_text);
			_locationText = FindViewById<TextView>(Resource.Id.location_text);
			FindViewById<TextView>(Resource.Id.search_gas_button).Click += SearchGas_OnClick;

			List<Place> places = new List<Place> ();
			markers = new List<Marker> ();

			Algorithm algorithm = new Algorithm ();
			Task<List<Place>> result = Task.Run(() => algorithm.Run ());
			Task.WaitAll (result);
			places = result.Result;

			markPlaces (places);

			MapFragment mapFrag = (MapFragment) FragmentManager.FindFragmentById(Resource.Id.map);
			Android.Gms.Maps.GoogleMap map = mapFrag.Map;
			map.MarkerClick += MapMarkerClick;


			GeoPoint origin = Algorithm.getCurrentLocation ();
			GeoPoint endPoint = Algorithm.getFinalDestination ();
			string transportationMethod = "driving";

			Task<TravelData> result2 = Task.Run(() => algorithm.getOriginalRoute (transportationMethod, origin, endPoint));
			originalRoute = result2.Result;

			Task<TravelData> result3 = Task.Run (() => algorithm.getPitStopRoute (transportationMethod, origin, places[1].Location, endPoint)); 
			TravelData pitStopRoute = result3.Result;

			polyLines = new List<Polyline> ();

			drawMark ("origin", origin);
			drawMark ("finalDestionation", endPoint);
			drawRoute (originalRoute, true);

			InitializeLocationManager();
		}

		protected override void OnResume()
		{
			base.OnResume();
			_locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
		}

		protected override void OnPause()
		{
			base.OnPause();
			_locationManager.RemoveUpdates(this);
		}

		void InitializeLocationManager()
		{
			_locationManager = (LocationManager)GetSystemService(LocationService);
			Criteria criteriaForLocationService = new Criteria
			{
				Accuracy = Accuracy.Fine
			};
			IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

			if (acceptableLocationProviders.Any())
			{
				_locationProvider = acceptableLocationProviders.First();
			}
			else
			{
				_locationProvider = String.Empty;
			}
		}

		async void drawMark(string type, GeoPoint point)
		{
			MapFragment mapFrag = (MapFragment) FragmentManager.FindFragmentById(Resource.Id.map);
			Android.Gms.Maps.GoogleMap map = mapFrag.Map;
			Marker marker;
			MarkerOptions markerOptions; 

			switch (type) {
			case "origin":
				markerOptions = new MarkerOptions ();
				markerOptions.SetPosition (new LatLng (point.latitude, point.longitude));
				markerOptions.SetTitle ("Origin");
				marker = map.AddMarker(markerOptions);
				marker.SetIcon(BitmapDescriptorFactory.FromResource (Resource.Drawable.car));
				markers.Add (marker);
				break;
			case "finalDestionation":
				markerOptions = new MarkerOptions ();
				markerOptions.SetPosition (new LatLng (point.latitude, point.longitude));
				markerOptions.SetTitle ("Final Destination");
				marker =	map.AddMarker(markerOptions);
				marker.SetIcon(BitmapDescriptorFactory.FromResource (Resource.Drawable.CheckeredFlag));
				markers.Add (marker);
				break;
			case "pitStop":
				markerOptions = new MarkerOptions ();
				markerOptions.SetPosition (new LatLng (point.latitude, point.longitude));
				markerOptions.SetTitle ("Final Destination");
				marker =	map.AddMarker(markerOptions);
				markers.Add (marker);
				break;
			}

		}

		private void MapMarkerClick(object sender, GoogleMap.MarkerClickEventArgs markerClickEventArgs)
		{
			markerClickEventArgs.Handled = true;
			Marker marker = markerClickEventArgs.Marker;
			GeoPoint origin = Algorithm.getCurrentLocation();
			GeoPoint endPoint = Algorithm.getFinalDestination();
			GeoPoint pitStop = new GeoPoint (marker.Position.Latitude, marker.Position.Longitude);

			foreach (Polyline polyLine in polyLines) {
				polyLine.Remove ();
			}

			Algorithm algorithm = new Algorithm ();
			Task<TravelData> result = Task.Run(() => algorithm.getPitStopRoute ("driving", origin, pitStop, endPoint));
			TravelData pitStopRoute = result.Result;
			drawRoute (pitStopRoute, false);

			//drawRoute (originalRoute, true);

			//Toast.MakeText(this, "You clicked on Marker", ToastLength.Short).Show();
		}

		async void drawRoute(TravelData route, bool isOriginalRoute)
		{
			MapFragment mapFrag = (MapFragment) FragmentManager.FindFragmentById(Resource.Id.map);
			Android.Gms.Maps.GoogleMap map = mapFrag.Map;

			// DRAW THE ORIGIN POINT FIRST, THEN POLYLINE, ENDPOINT, POLYLINE, ENDPOINT...
			PolylineOptions polylineOptions = new PolylineOptions ();
			polylineOptions.Add (new LatLng (route.steps [0].originXY.latitude, route.steps [0].originXY.longitude)); 
			Polyline polyline = map.AddPolyline (polylineOptions);
			if (isOriginalRoute) {
				polyline.Color = Resources.GetColor (Resource.Color.wallet_holo_blue_light);
			} else {
				polyline.Color = Resources.GetColor (Resource.Color.abc_search_url_text_pressed);
			}
			polyline.Width = 15;
			polyLines.Add (polyline);

			foreach (Step step in route.steps) {
				polylineOptions = new PolylineOptions ();

				List<GeoPoint> stepPoints = Algorithm.decodePoints (step.polyLine);
				foreach (GeoPoint point in stepPoints) {
					polylineOptions.Add (new LatLng (point.latitude, point.longitude));
				}
				polylineOptions.Add (new LatLng (step.endpointXY.latitude, step.endpointXY.longitude));
				polyline = map.AddPolyline (polylineOptions);
				if (isOriginalRoute) {
					polyline.Color = Resources.GetColor (Resource.Color.wallet_holo_blue_light);
				} else {
					polyline.Color = Resources.GetColor (Resource.Color.abc_search_url_text_pressed);
				}polyline.Width = 15;
				polyLines.Add (polyline);
			}
		}

		async void markPlaces(List<Place> places)
		{
			MapFragment mapFrag = (MapFragment) FragmentManager.FindFragmentById(Resource.Id.map);
			Android.Gms.Maps.GoogleMap map = mapFrag.Map;

			foreach (Place place in places) 
			{
				drawMark ("pitStop", place.Location);
			}

			LatLng location2 = new LatLng(places[4].Location.latitude, places[4].Location.longitude);
			CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
			builder.Target(location2);
			builder.Zoom(12);
			//builder.Bearing(155);
			//builder.Tilt(65);
			CameraPosition cameraPosition = builder.Build();
			CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition (cameraPosition);

			map.MoveCamera (cameraUpdate);
		}

		async void SearchGas_OnClick(object sender, EventArgs eventArgs)
		{
			if (_currentLocation == null)
			{
				_addressText.Text = "Location is null";
				return;
			}

			Algorithm algorithm = new Algorithm ();

			GeoPoint currentLoc = new GeoPoint (_currentLocation.Latitude, _currentLocation.Longitude);
			GeoPoint destination = Algorithm.getFinalDestination ();

			TravelData currentRoute = await algorithm.getOriginalRoute("driving", currentLoc, destination);
			List<GeoPoint> pointsToSearch = algorithm.findPointsToSearch (currentRoute, 500, currentLoc);

			MapFragment mapFrag = (MapFragment) FragmentManager.FindFragmentById(Resource.Id.map);
			Android.Gms.Maps.GoogleMap map = mapFrag.Map;

			foreach (GeoPoint point in pointsToSearch) 
			{
				MarkerOptions markerOpt1 = new MarkerOptions();
				markerOpt1.SetPosition(new LatLng(point.latitude, point.longitude));
				markerOpt1.SetTitle("Test Marker");
				map.AddMarker(markerOpt1);

				CircleOptions circleOptions = new CircleOptions ();
				circleOptions.InvokeCenter (new LatLng(point.latitude, point.longitude));
				circleOptions.InvokeRadius (500);
				map.AddCircle (circleOptions);
			}

			LatLng location2 = new LatLng(pointsToSearch[3].latitude, pointsToSearch[7].longitude);
			CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
			builder.Target(location2);
			builder.Zoom(12);
			//builder.Bearing(155);
			//builder.Tilt(65);
			CameraPosition cameraPosition = builder.Build();
			CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition (cameraPosition);

			map.MoveCamera (cameraUpdate);
		}

		public Location getCurrentLocation()
		{
			return _currentLocation;
		}
	}
}


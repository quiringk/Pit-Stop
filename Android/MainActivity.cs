using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Net;
using Android;
using Android.Gms.Maps;

using Xamarin.Forms.Platform.Android;
using System.Net.Http;
using Android.Gms.Maps.Model;
using System.Collections.Generic;
using Android.Locations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using System.Collections.ObjectModel;

namespace PitStop.Droid
{
	[Activity (Label = "PitStop.Droid.Android", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : Activity
	{
		public List<Polyline> polyLines;
		public TravelData originalRoute;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			StartActivity (typeof(GetLocation));

			Xamarin.Forms.Forms.Init (this, bundle);

			AndroidLogger logger = new AndroidLogger ();
			Logger.init (logger);

//			List<Place> places = new List<Place> ();
//
//			Algorithm algorithm = new Algorithm ();
//			Task<List<Place>> result = Task.Run(() => algorithm.Run ());
//			Task.WaitAll (result);
//			places = result.Result;
//
//
//
////			ListOfPlaces placesView = new ListOfPlaces (places);
////			SetPage (placesView);
//
//			SetContentView (Resource.Layout.Main);
//			markPlaces (places);
//
//			MapFragment mapFrag = (MapFragment) FragmentManager.FindFragmentById(Resource.Id.map);
//			Android.Gms.Maps.GoogleMap map = mapFrag.Map;
//			map.MarkerClick += MapMarkerClick;
//
//
//			GeoPoint origin = Algorithm.getCurrentLocation ();
//			GeoPoint endPoint = Algorithm.getFinalDestination ();
//			string transportationMethod = "driving";
//
//			Task<TravelData> result2 = Task.Run(() => algorithm.getOriginalRoute (transportationMethod, origin, endPoint));
//			originalRoute = result2.Result;
//
//			Task<TravelData> result3 = Task.Run (() => algorithm.getPitStopRoute (transportationMethod, origin, places[1].Location, endPoint)); 
//			TravelData pitStopRoute = result3.Result;
//
//			polyLines = new List<Polyline> ();
//
//			drawMark ("origin", origin);
//			drawMark ("finalDestionation", endPoint);
//			drawRoute (originalRoute, true);
//			//drawRoute (pitStopRoute, false);
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
				break;
			case "finalDestionation":
				markerOptions = new MarkerOptions ();
				markerOptions.SetPosition (new LatLng (point.latitude, point.longitude));
				markerOptions.SetTitle ("Final Destination");
				marker =	map.AddMarker(markerOptions);
				marker.SetIcon(BitmapDescriptorFactory.FromResource (Resource.Drawable.CheckeredFlag));
				break;
			case "pitStop":
				markerOptions = new MarkerOptions ();
				markerOptions.SetPosition (new LatLng (point.latitude, point.longitude));
				markerOptions.SetTitle ("Final Destination");
				marker =	map.AddMarker(markerOptions);
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

//		async void markPoints()
//		{
//			MapFragment mapFrag = (MapFragment) FragmentManager.FindFragmentById(Resource.Id.map);
//			Android.Gms.Maps.GoogleMap map = mapFrag.Map;
//
//			Algorithm algorithm = new Algorithm ();
//
//			//GET TRAVEL DATA FOR THE ORIGINAL ROUTE
//			GeoPoint originXY = algorithm.getCurrentLocationCoordinates ();
//			GeoPoint finalDestinationXY = algorithm.getFinalDestinationCoordinates ();
//			string placeType = "gas_station";
//			string transportationMethod = "driving";
//			int searchRadius = 500;
//
//			TravelData originalRoute = await Algorithm.getTravelDetails (transportationMethod, originXY, finalDestinationXY);
//			decimal originalMinutes = Math.Round ((originalRoute.seconds / 60.0m), 0);
//
//			List<GeoPoint> pointsToSearch = algorithm.findPointsToSearch(originalRoute, searchRadius, originXY);
//			foreach (GeoPoint point in pointsToSearch) 
//			{
//				MarkerOptions markerOpt1 = new MarkerOptions();
//				markerOpt1.SetPosition(new LatLng(point.latitude, point.longitude));
//				markerOpt1.SetTitle("Test Marker");
//				map.AddMarker(markerOpt1);
//			}
//
//			LatLng location2 = new LatLng(pointsToSearch[3].latitude, pointsToSearch[7].longitude);
//			CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
//			builder.Target(location2);
//			builder.Zoom(12);
//			//builder.Bearing(155);
//			//builder.Tilt(65);
//			CameraPosition cameraPosition = builder.Build();
//			CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition (cameraPosition);
//
//			map.MoveCamera (cameraUpdate);
//		}

		public void launchMaps()
		{
			// RANDOM VIEWPOINT
//			var geoUri = Android.Net.Uri.Parse ("geo:45.517148,-122.619279");
//			var mapIntent = new Intent (Intent.ActionView, geoUri);
//			StartActivity (mapIntent);

			// NAVIGATION
//			var geoUri = Android.Net.Uri.Parse("google.navigation:q=45.448154,-122.843072");
//			var mapIntent = new Intent (Intent.ActionView, geoUri);
//			StartActivity (mapIntent);

		}
	}
}


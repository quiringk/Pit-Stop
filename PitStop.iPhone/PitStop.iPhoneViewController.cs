using System;
using System.Drawing;

using Foundation;
using UIKit;

using PitStop;

using Google.Maps;
using CoreLocation;
using MapKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Mono;

using Xamarin.Forms;

namespace PitStop.iPhone
{
	public partial class PitStop_iPhoneViewController : UIViewController
	{
		public PitStop_iPhoneViewController (IntPtr handle) : base (handle)
		{
		}

		#region View lifecycle

		//MapView mapView;	//google

		protected MKMapView map;		//iOS
		MKPolyline polylineOverlay;
		MKPolylineRenderer polylineRenderer;
		//PitStopAnnotationDelegate pitStopAnnotationDelegate;

		CLLocationCoordinate2D origin = new CLLocationCoordinate2D (45.523538, -122.623040);
		CLLocationCoordinate2D endpoint = new CLLocationCoordinate2D (45.504796, -122.638593);
		GeoPoint originGeo = new GeoPoint (45.523538, -122.623040);
		GeoPoint endpointGeo = new GeoPoint (45.504796, -122.638593);

		//public static LocationManager Manager { get; set;}

		public override void LoadView ()
		{
			//Yelp.SearchForPlaces ();

			// LOADS MAP VIEW WITH PLACE ANNOTATIONS
//			map = new MKMapView (UIScreen.MainScreen.Bounds);
//			map.Delegate = new MyMapDelegate ();
//			View = map;
//			loadAppleMaps ();

//			// LOADS LIST VIEW WITH PLACES
//			Algorithm algorithm = new Algorithm ();
//			Task<List<Place>> result = Task.Run(() => algorithm.Run (originGeo, endpointGeo));
//			Task.WaitAll (result);
//			List<Place> places = result.Result;

//			Manager = new LocationManager ();
//			Manager.StartLocationUpdates ();
		}

//		public void HandleLocationChanged (object sender, LocationUpdatedEventArgs e)
//		{
//			// Handle foreground updates
//			CLLocation location = e.Location;
//
//			CLLocationCoordinate2D startLoc = new CLLocationCoordinate2D(location.Coordinate.Latitude, location.Coordinate.Longitude);
//			var GPSMarker = Marker.FromPosition (startLoc);
//			GPSMarker.Map = mapView;
//
//			Console.WriteLine ("foreground updated");
//		}

		public void loadAppleMaps()
		{
			Algorithm algorithm = new Algorithm();

//			map.DidSelectAnnotationView += (s,e) => {
//				var sampleAnnotation = e.View.Annotation as PitStopAnnotation;
//				if (sampleAnnotation != null) {
//					TravelData pitStopRouteData = algorithm.getPitStopRoute("driving", originGeo, new GeoPoint(sampleAnnotation.Coordinate.Latitude, sampleAnnotation.Coordinate.Longitude), endpointGeo);
//
//					List<CLLocationCoordinate2D> pitStopCoordsList = new List<CLLocationCoordinate2D> ();
//					for (int i = 0; i < pitStopRouteData.steps.Count; i++)
//					{
//						Step step = pitStopRouteData.steps [i];
//						pitStopCoordsList.Add (new CLLocationCoordinate2D (pitStopRouteData.steps [i].originXY.latitude, pitStopRouteData.steps [i].originXY.longitude));
//						List<GeoPoint> polylinePoints = Algorithm.decodePoints (step.polyLine);
//						foreach (GeoPoint geoPoint in polylinePoints) {
//							CLLocationCoordinate2D coord = new CLLocationCoordinate2D (geoPoint.latitude, geoPoint.longitude);
//							pitStopCoordsList.Add (coord);
//						}
//						pitStopCoordsList.Add (new CLLocationCoordinate2D (pitStopRouteData.steps [i].endpointXY.latitude, pitStopRouteData.steps [i].endpointXY.longitude));
//					}
//					CLLocationCoordinate2D[] coordsArray = pitStopCoordsList.ToArray();
//
//					this.map.RemoveOverlay(polylineOverlay);
//					polylineOverlay = MKPolyline.FromCoordinates (coordsArray);
//					this.map.AddOverlay(polylineOverlay);
//
//					FinishAnnotation test = new FinishAnnotation("test", new CLLocationCoordinate2D(45.520305, -122.621248));
//					map.AddAnnotation(test);
//
//					//demo accessing the title of the selected annotation
//					Console.WriteLine ("{0} was tapped", sampleAnnotation.Title);
//				}
//			};

			var annotation1 = new PitStopAnnotation ("Start", origin);
			map.AddAnnotation (annotation1);

//			startFlag = new UIImageView ();
//			startFlag.ContentMode = UIViewContentMode.ScaleAspectFit;
//			UIImage startFlagImage = UIImage.FromFile ("image/venue.png");
//			startFlag.Image = startFlagImage;

			var annotation2 = new FinishAnnotation ("FinishAnnotation", endpoint);
			map.AddAnnotation (annotation2);

			double centerLatitude = (annotation1.Coordinate.Latitude + annotation2.Coordinate.Latitude) / 2.0;
			double centerLongitude = (annotation1.Coordinate.Longitude + annotation2.Coordinate.Longitude) / 2.0;
			MKCoordinateSpan coordinateSpan = new MKCoordinateSpan ();
			if (centerLatitude < annotation1.Coordinate.Latitude) {
				coordinateSpan.LatitudeDelta = (annotation1.Coordinate.Latitude - centerLatitude) * 4.0;
			}else{
				coordinateSpan.LatitudeDelta = (annotation2.Coordinate.Latitude - centerLatitude) * 4.0;
			}
			if (centerLongitude < annotation1.Coordinate.Longitude) {
				coordinateSpan.LongitudeDelta = (annotation1.Coordinate.Longitude - centerLongitude) * 4.0;
			}else{
				coordinateSpan.LongitudeDelta = (annotation2.Coordinate.Longitude - centerLongitude) * 4.0;
			}

			map.Region = new MKCoordinateRegion(new CLLocationCoordinate2D(centerLatitude, centerLongitude), coordinateSpan);


			Task<List<Place>> placesFound = algorithm.Run (new GeoPoint (annotation1.Coordinate.Latitude, annotation1.Coordinate.Longitude), new GeoPoint (annotation2.Coordinate.Latitude, annotation2.Coordinate.Longitude));
			foreach (Place place in placesFound.Result)
			{
				var placeAnnotation = new PitStopAnnotation (place.Name, new CLLocationCoordinate2D (place.Location.latitude, place.Location.longitude));
				map.AddAnnotation (placeAnnotation);
			}

			TravelData travelData = algorithm.getOriginalRoute ("driving", new GeoPoint (annotation1.Coordinate.Latitude, annotation1.Coordinate.Longitude), new GeoPoint (annotation2.Coordinate.Latitude, annotation2.Coordinate.Longitude));

			List<CLLocationCoordinate2D> coordsList = new List<CLLocationCoordinate2D> ();
			for (int i = 0; i < travelData.steps.Count; i++)
			{
				Step step = travelData.steps [i];
				coordsList.Add (new CLLocationCoordinate2D (travelData.steps [i].originXY.latitude, travelData.steps [i].originXY.longitude));
				List<GeoPoint> polylinePoints = Algorithm.decodePoints (step.polyLine);
				foreach (GeoPoint geoPoint in polylinePoints) {
					CLLocationCoordinate2D coord = new CLLocationCoordinate2D (geoPoint.latitude, geoPoint.longitude);
					coordsList.Add (coord);
				}
				coordsList.Add (new CLLocationCoordinate2D (travelData.steps [i].endpointXY.latitude, travelData.steps [i].endpointXY.longitude));
			}
			CLLocationCoordinate2D[] coords = coordsList.ToArray ();

			polylineOverlay = MKPolyline.FromCoordinates (coords);
			map.AddOverlay (polylineOverlay);
		}




//		public class PitStopAnnotationDelegate : MKMapViewDelegate
//		{
//			public override void DidSelectAnnotationView (
//				MKMapView mapView, MKAnnotationView annotationView)
//			{
//				var sampleAnnotation =
//					annotationView.Annotation as PitStopAnnotation;
//
//				if (sampleAnnotation != null) {
//
//					//demo accessing the coordinate of the selected annotation to
//					//zoom in on it
//					mapView.Region = MKCoordinateRegion.FromDistance(
//						sampleAnnotation.Coordinate, 500, 500);
//
//					//demo accessing the title of the selected annotation
//					Console.WriteLine ("{0} was tapped", sampleAnnotation.Title);
//				}
//			}
//		}

//		public void loadGoogleMaps()
//		{
//			var camera = CameraPosition.FromCamera (latitude: 45.797865, 
//				longitude: -122.402526, 
//				zoom: 12);
//			//RectangleF rectangle = new RectangleF (45.45f, -122.65f, .05f, .05f);
//			mapView = MapView.FromCamera (RectangleF.Empty, camera);
//			mapView.MyLocationEnabled = true;
//
//			CLLocationCoordinate2D startLoc = new CLLocationCoordinate2D(45.514969, -122.683135);
//			var startMarker = Marker.FromPosition (startLoc);
//			startMarker.Title = string.Format ("Start");
//			startMarker.Map = mapView;
//			CLLocationCoordinate2D finishLoc = new CLLocationCoordinate2D(45.470332, -122.690114);
//			var finishMarker = Marker.FromPosition (finishLoc);
//			finishMarker.Title = string.Format ("Finish");
//			finishMarker.Map = mapView;
//
//			Google.Maps.CoordinateBounds coordinates = new CoordinateBounds (finishLoc, startLoc);
//			mapView.MoveCamera (CameraUpdate.FitBounds (coordinates));
//
//			mapView.Animate (12f);
//			//mapView.SetMinMaxZoom (14, 100);
//			View = mapView;
//		}

		#endregion
	}
}


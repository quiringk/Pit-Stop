using System;
using MapKit;
using UIKit;
using System.Drawing;
using CoreLocation;
using System.Collections.Generic;


namespace PitStop.iPhone
{
	public class MyMapDelegate: MKMapViewDelegate
	{
		static string annotationId = "FinishAnnotation";

		public MyMapDelegate(){
			Console.WriteLine ("test");
		}

		public override MKOverlayRenderer OverlayRenderer (MKMapView mapView, IMKOverlay overlay)
		{
			MKPolylineRenderer polylineRenderer = new MKPolylineRenderer(overlay as MKPolyline);
			polylineRenderer.StrokeColor = UIColor.Blue;
			polylineRenderer.LineWidth = 2f;
			return polylineRenderer;
		}

		public override void DidSelectAnnotationView (MKMapView mapView, MKAnnotationView view)
		{
			Algorithm algorithm = new Algorithm ();
			CLLocationCoordinate2D origin = new CLLocationCoordinate2D (45.523538, -122.623040);
			CLLocationCoordinate2D endpoint = new CLLocationCoordinate2D (45.504796, -122.638593);
			GeoPoint originGeo = new GeoPoint (45.523538, -122.623040);
			GeoPoint endpointGeo = new GeoPoint (45.504796, -122.638593);

			var sampleAnnotation = view.Annotation as PitStopAnnotation;
			if (sampleAnnotation != null) {
				TravelData pitStopRouteData = algorithm.getPitStopRoute("driving", originGeo, new GeoPoint(sampleAnnotation.Coordinate.Latitude, sampleAnnotation.Coordinate.Longitude), endpointGeo);

				List<CLLocationCoordinate2D> pitStopCoordsList = new List<CLLocationCoordinate2D> ();
				for (int i = 0; i < pitStopRouteData.steps.Count; i++)
				{
					Step step = pitStopRouteData.steps [i];
					pitStopCoordsList.Add (new CLLocationCoordinate2D (pitStopRouteData.steps [i].originXY.latitude, pitStopRouteData.steps [i].originXY.longitude));
					List<GeoPoint> polylinePoints = Algorithm.decodePoints (step.polyLine);
					foreach (GeoPoint geoPoint in polylinePoints) {
						CLLocationCoordinate2D coord = new CLLocationCoordinate2D (geoPoint.latitude, geoPoint.longitude);
						pitStopCoordsList.Add (coord);
					}
					pitStopCoordsList.Add (new CLLocationCoordinate2D (pitStopRouteData.steps [i].endpointXY.latitude, pitStopRouteData.steps [i].endpointXY.longitude));
				}
				CLLocationCoordinate2D[] coordsArray = pitStopCoordsList.ToArray();

				mapView.RemoveOverlay (mapView.Overlays [0]);
				MKPolyline newOverlay = MKPolyline.FromCoordinates (coordsArray);
				mapView.AddOverlay(newOverlay);

				//demo accessing the title of the selected annotation
				Console.WriteLine ("{0} was tapped", sampleAnnotation.Title);
			}
		}

		public override MKAnnotationView GetViewForAnnotation (MKMapView mapView, IMKAnnotation annotation)
		{
			MKAnnotationView annotationView = null;

			if (annotation is FinishAnnotation) {

				// show conference annotation
				annotationView = mapView.DequeueReusableAnnotation (annotationId);

				if (annotationView == null)
					annotationView = new MKAnnotationView (annotation, annotationId);

				annotationView.Image = UIImage.FromFile ("FinishFlag.jpg").Scale (new SizeF () { Height = 20, Width = 30 });
				annotationView.CanShowCallout = false;
			} 

			return annotationView;
		}

	}
}


using System;
using MapKit;
using CoreGraphics;
using UIKit;
using Foundation;
using CoreLocation;

namespace PitStop.iPhone
{
	public class FinishAnnotation : MKAnnotation
	{
		string title;
		CLLocationCoordinate2D coord;

		public FinishAnnotation (string title,
			CLLocationCoordinate2D coord)
		{
			this.title = title;
			this.coord = coord;
		}

		public override string Title {
			get {
				return title;
			}
		}

		public override CLLocationCoordinate2D Coordinate {
			get {
				return coord;
			}
		}
	}
}


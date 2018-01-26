using System;
using MapKit;
using CoreLocation;
using UIKit;
using Foundation;

namespace PitStop.iPhone
{
	public class PitStopAnnotation : MKAnnotation
	{
		string title;
		CLLocationCoordinate2D coord;

		public PitStopAnnotation (string title,
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


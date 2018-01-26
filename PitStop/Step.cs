using System;

namespace PitStop
{
	public class Step
	{
		public GeoPoint originXY { get; set; }
		public GeoPoint endpointXY { get; set; }
		public string polyLine { get; set; }
		public int distance { get; set; }
	}
}


using System;

namespace PitStop
{
	public class GeoPoint
	{
		public double latitude;
		public double longitude;

		public GeoPoint(double latValue, double longValue)
		{
			latitude = latValue;
			longitude = longValue;
		}
	}
}


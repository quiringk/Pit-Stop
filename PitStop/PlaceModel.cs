using System;
using Xamarin.Forms;

namespace PitStop
{
	public class Place
	{
		public string id { get; set; }
		public string Name { get; set; }
		public string openTime { get; set; }
		public string CloseTime { get; set; }
		public string AddedTime {get; set;}
		public bool openNow { get; set; }
		public bool googleHasHours { get; set; }
		public GeoPoint Location {get; set;}

		public Color OpenColor { get; set; }

		public ImageSource MapIcon { get; set; }
		public Aspect IconAspect { get; set; }

		public decimal Minutes { get; set; }
		decimal miles { get; set; }
		public int SecondsAdded { get; set; }

		public int RowHeight { get; set; }
	}
}


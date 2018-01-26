using System;
using System.Collections.Generic;

namespace PitStop
{
	public class TravelData 
	{
		public int meters { get; set; }
		public int seconds { get; set; }

		public List<Step> steps { get; set; }
	}
}


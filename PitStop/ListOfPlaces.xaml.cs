using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace PitStop
{	
	public partial class ListOfPlaces : ContentPage
	{	
		public ListOfPlaces (List<Place> places)
		{
			InitializeComponent ();
			this.BindingContext = places;
		}
	}
}


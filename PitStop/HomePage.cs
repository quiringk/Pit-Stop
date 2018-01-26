using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace PitStop
{
	class HomePage : ContentPage
	{
		Map map1;

		public HomePage()
		{
			Label header = new Label
			{
				Text = "Pit Stop",
				Font = Font.SystemFontOfSize(40),
				HorizontalOptions = LayoutOptions.Center
			};

			map1 = new  Map(MapSpan.FromCenterAndRadius(
				new Position(45.517148,-122.619279), Distance.FromMiles(0.3))) {
				IsShowingUser = false,
			};

			Button label1 = new Button
			{
				Text = "Navigate 2 Gas",
				Font = Font.SystemFontOfSize(NamedSize.Large)
			};

			// Accomodate iPhone status bar.
			this.Padding = new Thickness(10, Device.OnPlatform(20, 0, 0), 10, 5);

			// Build the page.
			this.Content = new StackLayout
			{
				Children = 
				{
					header,
					map1,
					label1
				}
				};
		}

		public void addPin(double longitude, double latitude)
		{
			var position = new Position(longitude, latitude); // Latitude, Longitude
			var pin = new Pin {
				Type = PinType.Place,
				Position = position,
				Label = "custom pin",
				Address = "custom detail info"
			};
			map1.Pins.Add(pin);

		}
	}
}


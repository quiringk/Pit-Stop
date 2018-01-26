using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Net.Http;

namespace PitStop
{
	public class App
	{


		public static Page GetMainPage ()
		{	
			HomePage homePage = new HomePage ();

//			return new ContentPage {
//				Content = new Label {
//					Text = "Hello, Forms !",
//					VerticalOptions = LayoutOptions.CenterAndExpand,
//					HorizontalOptions = LayoutOptions.CenterAndExpand,
//				},
//			};

			return homePage;
		}
	}
}


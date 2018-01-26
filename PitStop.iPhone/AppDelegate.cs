using System;
using System.Linq;
using System.Collections.Generic;

using Foundation;
using UIKit;

using Google.Maps;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace PitStop.iPhone
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.



	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		const string MapsApiKey = "";
		
		public override UIWindow Window {
			get;
			set;
		}
		
		// This method is invoked when the application is about to move from active to inactive state.
		// OpenGL applications should use this method to pause.
		public override void OnResignActivation (UIApplication application)
		{
		}
		
		// This method should be used to release shared resources and it should store the application state.
		// If your application supports background exection this method is called instead of WillTerminate
		// when the user quits.
		public override void DidEnterBackground (UIApplication application)
		{
		}
		
		// This method is called as part of the transiton from background to active state.
		public override void WillEnterForeground (UIApplication application)
		{
		}
		
		// This method is called when the application is about to terminate. Save data, if needed.
		public override void WillTerminate (UIApplication application)
		{
		}

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			Forms.Init();

			UIWindow window = new UIWindow(UIScreen.MainScreen.Bounds);

			Algorithm algorithm = new Algorithm ();
			GeoPoint originGeo = new GeoPoint (45.523538, -122.623040);
			GeoPoint endpointGeo = new GeoPoint (45.504796, -122.638593);
			Task<List<Place>> places = algorithm.Run (originGeo, endpointGeo);
			ListOfPlaces listOfPlaces = new ListOfPlaces (places.Result);

			window.RootViewController = listOfPlaces.CreateViewController();

			window.MakeKeyAndVisible();

			return true;
		}
	}
}


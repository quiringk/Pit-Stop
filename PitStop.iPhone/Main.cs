using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using PitStop;

using System.Threading.Tasks;

namespace PitStop.iPhone
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main (string[] args)
		{
//			List<Place> places = new List<Place> ();
//
//			Algorithm algorithm = new Algorithm ();
			//			places = algorithm.Run ();


			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main (args, null, "AppDelegate");


		}
	}
}

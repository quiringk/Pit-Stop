using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using ModernHttpClient;
using System.Net.Http;
using Xamarin.Forms;


namespace PitStop
{

	//  ALGORITHM:
	//
	//  1) GET GOOGLE DIRECTIONS FROM ORIGIN TO FINAL DESTINATION. THE DIRECTIONS RETURN MULTIPLE LEGS (LEG = ONE TURN TO ANOTHER) WITH THE GEOPOINTS OF EACH TURN
	//
	//  2) CIRCLE AROUND FIRST LEG ORIGIN
	//
	//  3) IF ENDPOINT OF THE CURRENT LEG IS < X METERS AWAY, GO BACK TO STEP 2 FOR THE NEXT LEG AND MOVE ON AND CONTINUE
	//
	//  4) IF ENDPOINT OF LEG IS > X MILES AWAY, CIRCLE X METERS LATER (AKA geoPointsInterval FURTHER IN THE geoPointList)
	//
	//  5) CONTINUE DOING STEP 4 UNTIL THE CURRENT CIRCLE IS < X METERS FROM ENDPOINT. THEN CONTINUE TO STEP 2 UNTIL YOU CIRCLE THE FINAL DESTINATION

	public class Algorithm
	{
		public List<Place> globalPlaces;

		int numTimesSearchPlacesCalled = 0;

		public async Task<List<Place>> Run(GeoPoint startCoordinate, GeoPoint endCoordinate)
		// I think pass the coordinates as params because startCoord will probably be pulled
		// from the native phones GPS
		//public List<Place> Run(GeoPoint startCoordinate, GeoPoint endCoordinate)
		{
			DateTime timeStarted = DateTime.Now;

			//GET TRAVEL DATA FOR THE ORIGINAL ROUTE
			GeoPoint originXY = startCoordinate;//getCurrentLocation ();
			GeoPoint finalDestinationXY = endCoordinate;//getFinalDestination ();
			string placeType = "thai food";
			string transportationMethod = "driving";
			int searchRadius = 100;

			TravelData originalRoute = getOriginalRoute (transportationMethod, originXY, finalDestinationXY);
			decimal originalMinutes = Math.Round ((originalRoute.seconds / 60.0m), 0);

			List<GeoPoint> pointsToSearch = findPointsToSearch(originalRoute, searchRadius, originXY);
//			foreach (GeoPoint point in pointsToSearch) 
//			{
//				Logger.info ("SearchPoint", point.latitude + "," + point.longitude);
//			}
			
			List<List<GeoPoint>> pointsArrays = new List<List<GeoPoint>>();
			List<GeoPoint> pointsArray1 = new List<GeoPoint>();
			List<GeoPoint> pointsArray2 = new List<GeoPoint>();
			List<GeoPoint> pointsArray3 = new List<GeoPoint>();
			List<GeoPoint> pointsArray4 = new List<GeoPoint>();
			List<GeoPoint> pointsArray5 = new List<GeoPoint>();
			List<GeoPoint> pointsArray6 = new List<GeoPoint>();
			List<GeoPoint> pointsArray7 = new List<GeoPoint>();
			List<GeoPoint> pointsArray8 = new List<GeoPoint>();
			pointsArrays.Add(pointsArray1);
			pointsArrays.Add(pointsArray2);
			pointsArrays.Add(pointsArray3);
			pointsArrays.Add(pointsArray4);
			pointsArrays.Add(pointsArray5);
			pointsArrays.Add(pointsArray6);
			pointsArrays.Add(pointsArray7);
			pointsArrays.Add(pointsArray8);

			int numberPointsPerTask = pointsToSearch.Count / pointsArrays.Count;
			numberPointsPerTask++;
			int numPointsAdded = 0;
			int pointsArraysIndex = 0;
			for(int i = 0; i < pointsToSearch.Count; i++)
			{
				if(numPointsAdded == numberPointsPerTask)
				{
					numPointsAdded = 0;
					pointsArraysIndex++;
				}
				pointsArrays[pointsArraysIndex].Add(pointsToSearch[i]);
				numPointsAdded++;
			}

//			List<List<Place>> allPlaces = new List<List<Place>> ();
//			List<Place> places1 = await searchPlacesFromListPoints (pointsArray1, searchRadius, placeType);
//			List<Place> places2 = await searchPlacesFromListPoints (pointsArray2, searchRadius, placeType);
//			List<Place> places3 = await searchPlacesFromListPoints (pointsArray3, searchRadius, placeType);
//			List<Place> places4 = await searchPlacesFromListPoints (pointsArray4, searchRadius, placeType);
//			allPlaces.Add (places1);
//			allPlaces.Add (places2);
//			allPlaces.Add (places3);
//			allPlaces.Add (places4);

			Task<List<Place>>[] allPlaces = {
				Task<List<Place>>.Run(() => runTasksSearchPlaces(pointsArray1, searchRadius, placeType)),
				Task<List<Place>>.Run(() => runTasksSearchPlaces(pointsArray2, searchRadius, placeType)),
				Task<List<Place>>.Run(() => runTasksSearchPlaces(pointsArray3, searchRadius, placeType)),
				Task<List<Place>>.Run(() => runTasksSearchPlaces(pointsArray4, searchRadius, placeType)),
				Task<List<Place>>.Run(() => runTasksSearchPlaces(pointsArray5, searchRadius, placeType)),
				Task<List<Place>>.Run(() => runTasksSearchPlaces(pointsArray6, searchRadius, placeType)),
				Task<List<Place>>.Run(() => runTasksSearchPlaces(pointsArray7, searchRadius, placeType)),
				Task<List<Place>>.Run(() => runTasksSearchPlaces(pointsArray8, searchRadius, placeType))
			};
			Task.WaitAll (allPlaces);

			List<Place> placesToSearch = new List<Place>();
			for(int x = 0; x < allPlaces.Length; x++)
			{
				foreach(Place place in allPlaces[x].Result)
				{
					placesToSearch.Add(place);
				}
			}
			placesToSearch = removeDuplicatePlaces(placesToSearch);

			int index = 0;
			List<Place> placesToSearch2 = new List<Place> ();
			foreach (Place place in placesToSearch) 
			{
				placesToSearch2.Add (getPlaceDetails (place.id));
			}

			TimeSpan duration = DateTime.Now - timeStarted;



			Logger.info ("Results", "Number Of Places Found: " + placesToSearch2.Count);
			Logger.info ("Results", "Place Searches Calls: " + numTimesSearchPlacesCalled);
			Logger.info ("Results", "Place Details Calls: " + placesToSearch2.Count);
			Logger.info ("Results", "Total API Calls: " + (placesToSearch2.Count + numTimesSearchPlacesCalled));
			Logger.info ("Results", "Total Time: " + duration.ToString());

			foreach (Place place in placesToSearch2)
			{
				Logger.info ("Results", " ");
				Logger.info ("Results", place.Name);
				Logger.info ("Results", "Open now: " + place.openNow);
				Logger.info ("Results", "Open: " + place.openTime);
				Logger.info ("Results", "Close: " + place.CloseTime);
				Logger.info ("Results", "Coordinates: " + place.Location.latitude + "," + place.Location.longitude);
			}

			foreach (Place place in placesToSearch2) {
				if (place.openNow) {
					place.OpenColor = Color.Green;
				}else{
					place.OpenColor = Color.Red;
				}

				TravelData travelData = getPitStopRoute (transportationMethod, originXY, place.Location, finalDestinationXY);
				int pitStopAddedSeconds = travelData.seconds - originalRoute.seconds;
				place.SecondsAdded = pitStopAddedSeconds;
				if (pitStopAddedSeconds > 60) {
					int minsAdded = pitStopAddedSeconds / 60;
					place.AddedTime = minsAdded + " Mins";
				} else {
					place.AddedTime = pitStopAddedSeconds + " Seconds";
				}
			}

			return placesToSearch2;
		}

		public static GeoPoint getCurrentLocation()
		{
			GeoPoint geoPoint = new GeoPoint (45.516218, -122.619408);
			return geoPoint;
		}

		public static GeoPoint getFinalDestination()
		{
			GeoPoint geoPoint = new GeoPoint (45.533429, -122.554858);	// BURNSIDE & CESAR
			return geoPoint;
		}

		public List<GeoPoint> findPointsToSearch(TravelData originalRoute, int searchRadius, GeoPoint originXY)
		{
			// START OFF BY ADDING THE ORIGIN POINT
			List<GeoPoint> pointsToSearch = new List<GeoPoint>();
			pointsToSearch.Add(originXY);

			// CONTINUE SEARCHING ALONG THE NEXT STEP UNTIL FOUND ALL POINTS
			for (int i = 0; i < originalRoute.steps.Count; i++)
			{
				List<GeoPoint> morePoints = findPointsAlongStep(originalRoute.steps[i], searchRadius);
				foreach (GeoPoint point in morePoints)
				{
					pointsToSearch.Add(point);
				}
			}

			return pointsToSearch;
		}

		public List<GeoPoint> findPointsAlongStep(Step step, int searchRadius)
		{
			List<GeoPoint> pointsToSearch = new List<GeoPoint>();

			// ONLY NEED TO FIND POINTS ON A STEP IF THE STEP IS GREATHER THAN THE SEARCH RADIUS
			if (step.distance > searchRadius)
			{
				pointsToSearch = computeStepSearchPoints(step, searchRadius);
			}
			pointsToSearch.Add(step.endpointXY);

			return pointsToSearch;
		}

		public async Task<List<Place>> runTasksSearchPlaces(List<GeoPoint> points, int searchRadius, string placeType)
		{
			List<Place> allPlacesFound = new List<Place>();

			List<List<GeoPoint>> pointsArrays = new List<List<GeoPoint>>();
			List<GeoPoint> pointsArray1 = new List<GeoPoint>();
			List<GeoPoint> pointsArray2 = new List<GeoPoint>();
			List<GeoPoint> pointsArray3 = new List<GeoPoint>();
			pointsArrays.Add(pointsArray1);
			pointsArrays.Add(pointsArray2);
			pointsArrays.Add(pointsArray3);

			int numberPointsPerTask = points.Count / pointsArrays.Count;
			numberPointsPerTask++;
			int numPointsAdded = 0;
			int pointsArraysIndex = 0;
			for(int i = 0; i < points.Count; i++)
			{
				if(numPointsAdded == numberPointsPerTask)
				{
					numPointsAdded = 0;
					pointsArraysIndex++;
				}
				pointsArrays[pointsArraysIndex].Add(points[i]);
				numPointsAdded++;
			}

			Task<List<Place>>[] allPlaces = {
				Task<List<Place>>.Run(() => searchPlacesFromPoints(pointsArray1, searchRadius, placeType)),
				Task<List<Place>>.Run(() => searchPlacesFromPoints(pointsArray2, searchRadius, placeType)),
				Task<List<Place>>.Run(() => searchPlacesFromPoints(pointsArray3, searchRadius, placeType))
			};
			Task.WaitAll (allPlaces);

			foreach (Task<List<Place>> list in allPlaces) {
				List<Place> listPlaces = list.Result;
				foreach(Place place in listPlaces)
				{
					allPlacesFound.Add (place);
				}
			}

			return allPlacesFound;
		}

		public List<Place> searchPlacesFromPoints(List<GeoPoint> points, int searchRadius, string placeType)
		{
			List<Place> placesList = new List<Place> ();

			foreach (GeoPoint point in points) {
				List<Place> places = searchPlacesFromPoint (point, searchRadius, placeType);
				foreach(Place place in places)
				{
					placesList.Add(place);
				}
			}

 			return placesList;
		}



		public List<Place> searchPlacesFromPoint(GeoPoint coordinates, int searchRadius, string searchType)
		{
			List<Place> placesToAdd = new List<Place>();

			string url = @"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + coordinates.latitude + "," + coordinates.longitude + "&keyword=" + searchType + "&rankby=distance&key=";
			var httpClient = new HttpClient(new NativeMessageHandler());
			Task<HttpResponseMessage> response = null;
			response = httpClient.GetAsync(url);

			Task<string> returnedText = response.Result.Content.ReadAsStringAsync ();
			var json = (JObject)JsonConvert.DeserializeObject(returnedText.Result);

			if ((string)json["status"] == "OK")
			{
				var places = json["results"];
				int index = 0;
				foreach (JToken place in places)
				{
					Place nextPlace = new Place();
					string placeId = (string)place["place_id"];
					nextPlace.id = placeId;
					placesToAdd.Add(nextPlace);
					if (index == 3) 
					{
						index = 0;
						break;
					}
					index++;
				}
			}
			else
			{
				// Error Handling
			}

			numTimesSearchPlacesCalled++;
			return placesToAdd;
		}

		public List<GeoPoint> computeStepSearchPoints(Step step, int searchRadius)
		{
			List<GeoPoint> pointsToSearch = new List<GeoPoint>();

			List<GeoPoint> allPointsAlongStep = decodePoints(step.polyLine);
			int distanceCovered = searchRadius; //THE FIRST POINT HAS ALREADY BEEN SEARCHED
			int numSearches = step.distance / searchRadius;
			numSearches++;  //ALWAYS ERROR ON THE SIDE OF MORE SEARCHES VS LESS SEARCHES
			int geoPointsInterval = allPointsAlongStep.Count / numSearches;
			int index = geoPointsInterval;
			while ((step.distance - distanceCovered) > searchRadius)
			{
				pointsToSearch.Add(allPointsAlongStep[index]);
				index += geoPointsInterval;
				distanceCovered += searchRadius;
			}

			return pointsToSearch;
		}

		public List<Place> removeDuplicatePlaces(List<Place> allPlaces)
		{
			List<Place> placesDuplicatesRemoved = new List<Place>();

			bool duplicateFound;
			for (int i = 1; i < allPlaces.Count; i++)
			{
				duplicateFound = false;
				for (int x = 0; x < placesDuplicatesRemoved.Count; x++)
				{
					if (allPlaces[i].id == placesDuplicatesRemoved[x].id)
					{
						duplicateFound = true;
						break;
					}
				}
				if (duplicateFound)
				{
					continue;
				}
				else
				{
					placesDuplicatesRemoved.Add(allPlaces[i]);
				}
			}

			return placesDuplicatesRemoved;
		}

		public bool placeAlreadyAdded(List<Place> allPlaces, string placeId)
		{
			foreach (Place place in allPlaces) 
			{
				if (place.id == placeId) 
				{
					return true;
				}
			}
			return false;
		}

		public static Place getPlaceDetails(string placeId)
		{
			string url = @"https://maps.googleapis.com/maps/api/place/details/json?placeid=" + placeId + "&key=";
			var httpClient = new HttpClient(new NativeMessageHandler());
			Task<HttpResponseMessage> response = httpClient.GetAsync(url);
			Task<string> returnedText = response.Result.Content.ReadAsStringAsync ();
			var json = (JObject)JsonConvert.DeserializeObject(returnedText.Result);

			string status = (string)json["status"];
			Place place = new Place();
			if(status == "OK")
			{
				var placeToStop = json["result"];
				try
				{
					place.id = (string)placeToStop["place_id"];
					place.Name = (string)placeToStop["name"];
				}
				catch
				{

				}

				// OPEN HOURS
				try
				{
					var openingHours = placeToStop["opening_hours"];
					if(openingHours == null){
						place.googleHasHours = false;
					}else{
						place.googleHasHours = true;
					}
					place.openNow = Convert.ToBoolean((string)openingHours["open_now"]);
					var days = openingHours["periods"];
					foreach (JToken day in days)
					{
						var open = day["open"];
						string openTime = (string)open["time"];
						openTime = openTime.Insert(2, ":");
						place.openTime = DateTime.Parse(openTime).ToString(@"hh\:mm\:ss tt");
						var close = day["close"];
						string closeTime = (string)close["time"];
						closeTime = closeTime.Insert(2, ":");
						place.CloseTime = DateTime.Parse(closeTime).ToString(@"hh\:mm\:ss tt");
					}
				}
				catch (Exception ex)
				{
					//Console.WriteLine("No open hours were provided");
				}



				// LONGITUDE + LATITUDE 
				var geometry = placeToStop["geometry"];
				var location = geometry["location"];
				place.Location = new GeoPoint((double)location["lat"], (double)location["lng"]);
		
				// PLACE ICON
				Uri iconUrl = (Uri)placeToStop ["icon"];
				//Image icon = new Image { Aspect = Aspect.AspectFit };
				place.IconAspect = Aspect.AspectFit;
				place.MapIcon = ImageSource.FromUri (iconUrl);
//				icon.Source = ImageSource.FromUri (iconUrl);
//				place.MapIcon = icon;

				place.RowHeight = 100;
			}
			else
			{
				//error handling
			}

			return place;
		}

		public TravelData getOriginalRoute(string travelMode, GeoPoint originXY, GeoPoint destinationXY)
		{
			TravelData travelData = new TravelData();
			travelData.steps = new List<Step> ();

			// ORIGIN -> PLACE DATA
			string url = @"https://maps.googleapis.com/maps/api/directions/json?mode=" + travelMode + "&origin=" + originXY.latitude + "," + originXY.longitude + "&destination=" + destinationXY.latitude + "," + destinationXY.longitude + "&key=";
			var httpClient = new HttpClient(new NativeMessageHandler());
			Task<HttpResponseMessage> response = httpClient.GetAsync(url);
			Task<string> returnedText = response.Result.Content.ReadAsStringAsync ();
			var json = (JObject)JsonConvert.DeserializeObject(returnedText.Result);

			string status = (string)json["status"];
			if (status == "OK")
			{
				try
				{
					var routes = json["routes"];
					var route = routes[0];
					var legs = route["legs"];
					var leg = legs[0];
					var totalDistance = leg["distance"];
					travelData.meters = (int)totalDistance["value"];
					var totalDuration = leg["duration"];
					travelData.seconds = (int)totalDuration["value"];

					var steps = leg["steps"];
					foreach(JToken step in steps)
					{
						Step nextStep = new Step();

						JToken startLoc = step["start_location"];
						nextStep.originXY = new GeoPoint((double)startLoc["lat"], (double)startLoc["lng"]);
						JToken endLoc = step["end_location"];
						nextStep.endpointXY = new GeoPoint((double)endLoc["lat"], (double)endLoc["lng"]);
						JToken polyLine = step["polyline"];
						nextStep.polyLine = (string)polyLine["points"];
						JToken distance = step["distance"];
						nextStep.distance = (int)distance["value"];

						travelData.steps.Add(nextStep);
					}
				}
				catch(Exception ex)
				{

				}
			}
			else
			{
				//error handling
			}
			return travelData;
		}
	
		public TravelData getPitStopRoute(string travelMode, GeoPoint originLoc, GeoPoint pitStopLoc, GeoPoint destinationLoc)
		{
			TravelData travelData = new TravelData();
			travelData.steps = new List<Step> ();

			// ORIGIN -> PLACE DATA
			string url = @"https://maps.googleapis.com/maps/api/directions/json?"
				+ "mode=" + travelMode
				+ "&origin=" + originLoc.latitude + "," + originLoc.longitude
				+ "&destination=" + destinationLoc.latitude + "," + destinationLoc.longitude
				+ "&waypoints=" + pitStopLoc.latitude + "," + pitStopLoc.longitude
				+ "&key=";
			var httpClient = new HttpClient(new NativeMessageHandler());
			Task<HttpResponseMessage> response = httpClient.GetAsync(url);
			Task<string> returnedText = response.Result.Content.ReadAsStringAsync ();
			var json = (JObject)JsonConvert.DeserializeObject(returnedText.Result);

			string status = (string)json["status"];
			if (status == "OK")
			{
				try
				{
					var routes = json["routes"];
					var route = routes[0];
					var legs = route["legs"];
					foreach(var leg in legs)
					{
						var totalDistance = leg["distance"];
						travelData.meters += (int)totalDistance["value"];
						var totalDuration = leg["duration"];
						travelData.seconds += (int)totalDuration["value"];

						var steps = leg["steps"];
						foreach(JToken step in steps)
						{
							Step nextStep = new Step();

							JToken startLoc = step["start_location"];
							nextStep.originXY = new GeoPoint((double)startLoc["lat"], (double)startLoc["lng"]);
							JToken endLoc = step["end_location"];
							nextStep.endpointXY = new GeoPoint((double)endLoc["lat"], (double)endLoc["lng"]);
							JToken polyLine = step["polyline"];
							nextStep.polyLine = (string)polyLine["points"];
							JToken distance = step["distance"];
							nextStep.distance = (int)distance["value"];

							travelData.steps.Add(nextStep);
						}
					}
				}
				catch(Exception ex)
				{

				}
			}
			else
			{
				//error handling
			}
			return travelData;
		}

		public static List <GeoPoint> decodePoints(String encoded_points)
		{
			int index = 0;
			int lat = 0;
			int lng = 0;
			List<GeoPoint> geoPoints = new List<GeoPoint>();

			try {
				int shift;
				int result;
				while (index < encoded_points.Length) {
					shift = 0;
					result = 0;
					while (true) {
						int b = encoded_points[index++] - '?';
						result |= ((b & 31) << shift);
						shift += 5;
						if (b < 32)
							break;
					}
					lat += ((result & 1) != 0 ? ~(result >> 1) : result >> 1);

					shift = 0;
					result = 0;
					while (true) {
						int b = encoded_points[index++] - '?';
						result |= ((b & 31) << shift);
						shift += 5;
						if (b < 32)
							break;
					}
					lng += ((result & 1) != 0 ? ~(result >> 1) : result >> 1);
					/* Add the new Lat/Lng to the Array. */
					geoPoints.Add(new GeoPoint((lat*10*.000001),(lng*10*.000001)));
				}
//				foreach(GeoPoint point in geoPoints)
//				{
//
//				}
				return geoPoints;
			}catch(Exception e) {
				//ERROR HANDLINGb
			}
			return geoPoints;
		}
	}
}


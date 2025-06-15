//using System.Diagnostics;
//using System.Net.Http.Json;
//using System.Net.Mime;
//using Bible.Backend.Passage;
//using Bible.Backend.Verse;

//var httpClient = new HttpClient();
//httpClient.BaseAddress = new Uri(DigitalBibleLibraryConstants.BaseUrl);
//httpClient.DefaultRequestHeaders.Add("accept", MediaTypeNames.Application.Json);

//Debug.WriteLine(DigitalBibleLibraryConstants.SampleFull);
//var response = await httpClient.GetStringAsync(DigitalBibleLibraryConstants.SampleSuffix);
//Debug.WriteLine(response);
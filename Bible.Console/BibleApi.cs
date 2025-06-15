//using System.Diagnostics;
//using System.Net.Http.Json;
//using System.Net.Mime;
//using Bible.Backend.Passage;
//using Bible.Backend.Verse;

//var httpClient = new HttpClient();
//httpClient.BaseAddress = new Uri(BibleApiConstants.BaseUrl);
//httpClient.DefaultRequestHeaders.Add("accept", MediaTypeNames.Application.Json);
//httpClient.DefaultRequestHeaders.Add("api-key", BibleApiConstants.Key);

//Debug.WriteLine(BibleApiConstants.SampleFull);
////var response = await httpClient.GetStringAsync(BibleApiConstants.SampleSuffix);
//var response = await httpClient.GetFromJsonAsync<PassageResponse>(BibleApiConstants.SampleSuffix);
////var content = response?.data.content?.SelectMany(o => o.items).Select(o => o.text);
////if (content == null) return;
////var text = string.Join("", content);
//Debug.WriteLine(response?.data.content);
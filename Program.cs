using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Read_Speaker_Bios
{
    class Program
    {
        const string sessionsURL = "https://sessionize.com/api/v2/...";
        static string CONSUMERKEY = "Your Twitter Consumer Key";
        static string CONSUMERKEYSECRET = "Your Twitter Consumer Secret";
        static string ACCESSTOKEN = "Your Twitter Access Token";
        static string ACCESSTOKENSECRET = "Your Twitter Access Token Secret";

        static void Main(string[] args)
        {
            //CallTwitter("test tweet");

            List<Peep> presenters = GetPeopleAndSessions();
            //SendPreEventTweets(presenters, 15);
            SendPostEventThankYous(presenters, 2);
        }
        private static void SendPreEventTweets(List<Peep> presenters, int minutesBetweenTweets)
        {
            foreach (var presenter in presenters)
            {
                foreach (var session in presenter.Sessions)
                {
                    string tweet = $"Come to #kcdc2019 to hear {presenter.FullName} {presenter.Twitter}speak about \"{session}\"";
                    Debug.WriteLine(tweet);
                    CallTwitter(tweet);
                    System.Threading.Thread.Sleep(1000 * 60 * minutesBetweenTweets);
                }
            }
        }

        private static void SendPostEventThankYous(List<Peep> presenters, int minutesBetweenTweets)
        {
            foreach (var presenter in presenters)
            {
                string tweet = $"Thank you {presenter.FullName} {presenter.Twitter}for speaking at #kcdc2019";
                Debug.WriteLine(tweet);
                CallTwitter(tweet);
                System.Threading.Thread.Sleep(1000 * 60 * minutesBetweenTweets);
            }
        }
        private static string GetTwitterHandleFromTwitterURL(string twitterURL)
        {
            return twitterURL
                .Replace("https://twitter.com/", "@", StringComparison.OrdinalIgnoreCase)
                .Replace("http://twitter.com/", "@", StringComparison.OrdinalIgnoreCase)
                .Replace("https://www.twitter.com/", "@", StringComparison.OrdinalIgnoreCase)
                .Replace("http://www.twitter.com/", "@", StringComparison.OrdinalIgnoreCase) + " ";
        }

        private static List<Peep> GetPeopleAndSessions()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(sessionsURL);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(sessionsURL).Result;

            List<Peep> peeps = new List<Peep>();
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                List<RootObject> speakers = JsonConvert.DeserializeObject<List<RootObject>>(result);
                foreach (var speaker in speakers)
                {
                    Peep newPeep = new Peep();
                    newPeep.FullName = speaker.fullName;
                    newPeep.ProfilePictureURL = speaker.profilePicture;
                    foreach (var link in speaker.links)
                    {
                        bool isTwitter = false;
                        string url = "";
                        Newtonsoft.Json.Linq.JObject vv = (Newtonsoft.Json.Linq.JObject)link;
                        foreach (var a in vv)
                        {
                            if (a.Key.Equals("linkType", StringComparison.OrdinalIgnoreCase))
                            {
                                if (a.Value.ToString().Equals("Twitter", StringComparison.OrdinalIgnoreCase))
                                    isTwitter = true;
                            }
                            if (a.Key.Equals("url", StringComparison.OrdinalIgnoreCase))
                            {
                                url = a.Value.ToString();
                            }
                        }
                        if (isTwitter)
                            newPeep.Twitter = GetTwitterHandleFromTwitterURL(url);
                    }
                    foreach (var session in speaker.sessions)
                    {
                        if (newPeep.Sessions == null)
                            newPeep.Sessions = new List<string>();
                        newPeep.Sessions.Add(session.name);

                    }
                    peeps.Add(newPeep);
                }
            }

            return peeps;
        }

        private static void CallTwitter(string message)
        {
            var twitter = new TwitterApi(CONSUMERKEY, CONSUMERKEYSECRET, ACCESSTOKEN, ACCESSTOKENSECRET);
            var response = twitter.Tweet(message).Result;
            Console.WriteLine(response);

        }
        #region helper struct
        public struct Peep
        {
            public string FullName { get; set; }
            string Bio { get; set; }
            string Tagline { get; set; }
            public string ProfilePictureURL { get; set; }
            public List<string> Sessions { get; set; }
            public List<string> Links { get; set; }
            public string Twitter { get; set; }
        }
        #endregion
    }
    #region Helper Classes
    public class Session
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class RootObject
    {
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string fullName { get; set; }
        public string bio { get; set; }
        public string tagLine { get; set; }
        public string profilePicture { get; set; }
        public List<Session> sessions { get; set; }
        public bool isTopSpeaker { get; set; }
        public List<object> links { get; set; }
        public List<object> questionAnswers { get; set; }
        public List<object> categories { get; set; }
    }
    #endregion
}

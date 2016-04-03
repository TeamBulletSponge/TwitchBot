using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;

namespace TwitchBot.Notification
{
  class Program
  {
    private static readonly string KrakenUrl = "https://api.twitch.tv/kraken/";
    private static readonly WebClient _webClient = new WebClient();
    private static HashSet<string> _channelNofitications = new HashSet<string>();

    static void Main(string[] args)
    {
      Console.Out.WriteLine("Endpoint: " + ConfigurationManager.AppSettings["SlackEndPoint"]);
      Console.Out.WriteLine("Channels: " + ConfigurationManager.AppSettings["TwitchChannels"]);

      string endpoint = ConfigurationManager.AppSettings["SlackEndPoint"];
      string[] channels = ConfigurationManager.AppSettings["TwitchChannels"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

      while (true)
      {
        foreach (string channel in channels)
        {
          string url = KrakenUrl + "streams/" + channel;
          var json = _webClient.DownloadString(url);
          JObject result = JObject.Parse(json);

          string message = null;

          if (result["stream"].HasValues && !_channelNofitications.Contains(channel))
          {
            message = "{\"text\": \"" + channel + " is broadcasting.\"}";
            _channelNofitications.Add(channel);
          }
          else if (!result["stream"].HasValues && _channelNofitications.Contains(channel))
          {
            message = "{\"text\": \"" + channel + " has stopped broadcasting.\"}";
            _channelNofitications.Remove(channel);
          }

          if (!string.IsNullOrEmpty(message))
          {
            WebRequest req = WebRequest.Create(endpoint);

            req.Proxy = null;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            byte[] reqData = Encoding.UTF8.GetBytes(message);
            req.ContentLength = reqData.Length;

            using (Stream reqStream = req.GetRequestStream())
            {
              reqStream.Write(reqData, 0, reqData.Length);
            }

            req.GetResponse();
          }
        }

        Thread.Sleep(TimeSpan.FromMinutes(2));
      }
    }
  }
}

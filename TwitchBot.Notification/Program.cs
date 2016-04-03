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
      string endpoint = ConfigurationManager.AppSettings["SlackEndPoint"];
      string channelList = ConfigurationManager.AppSettings["TwitchChannels"];
      string[] channels = channelList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

      Console.Out.WriteLine("Endpoint: " + endpoint);
      Console.Out.WriteLine("Channels: " + channelList);

      SendMessage(endpoint, "{\"text\": \"Started following channel(s): " + channelList + "\"}");

      while (true)
      {
        foreach (string channel in channels)
        {
          try
          {
            string url = KrakenUrl + "streams/" + channel;
            var json = _webClient.DownloadString(url);
            JObject result = JObject.Parse(json);

            string message = null;

            if (result["stream"].HasValues && !_channelNofitications.Contains(channel))
            {
              message = BuildOnlineMessage(channel, result);
              _channelNofitications.Add(channel);
            }
            else if (!result["stream"].HasValues && _channelNofitications.Contains(channel))
            {
              message = "{\"text\": \"" + channel + " has stopped broadcasting\"}";
              _channelNofitications.Remove(channel);
            }

            SendMessage(endpoint, message);
          }
          catch (Exception ex)
          {
            Console.Error.WriteLine(ex);
          }
        }

        Thread.Sleep(TimeSpan.FromMinutes(2));
      }
    }

    private static string BuildOnlineMessage(string channel, JObject result)
    {
      return "{\"attachments\": [{\"title\": \"" + channel + " is broadcasting '" + result["stream"]["channel"]["game"].ToString() + "'\", \"title_link\": \"http://twitch.tv/" + channel + "\", \"image_url\": \"" + result["stream"]["preview"]["medium"].ToString() + "\"}]}";
    }

    private static void SendMessage(string endpoint, string message)
    {
      try
      {
        if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(message))
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
      catch (Exception ex)
      {
        Console.Error.WriteLine(ex);
      }
    }
  }
}

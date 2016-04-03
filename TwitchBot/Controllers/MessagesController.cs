using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;

namespace TwitchBot
{
  [BotAuthentication]
  public class MessagesController : ApiController
  {
    private static readonly string KrakenUrl = "https://api.twitch.tv/kraken/";
    private static readonly WebClient WebClient = new WebClient();

    /// <summary>
    /// POST: api/Messages
    /// Receive a message from a user and reply to it
    /// </summary>
    public async Task<Message> Post([FromBody]Message message)
    {
      if (message.Type == "Message")
      {
        string[] tokens = message.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens == null || tokens.Length == 0)
        {
          return null;
        }

        if (tokens[0] == "status")
        {
          if (tokens.Length == 1)
          {
            return message.CreateReplyMessage("You must provide a channel name: /status {channel}");
          }

          string channel = tokens[1];
          string url = KrakenUrl + "streams/" + channel;
          var json = WebClient.DownloadString(url);
          JObject result = JObject.Parse(json);

          if (result["stream"].HasValues)
          {
            Attachment previewAttachment = new Attachment();
            previewAttachment.ContentUrl = result["stream"]["preview"]["medium"].ToString(); ;
            previewAttachment.ContentType = "image/png";
            Message reply = message.CreateReplyMessage(String.Format("[{0}]({1}{0}) is broadcasting '" + result["stream"]["channel"]["game"].ToString() + "' since {2}.{3}", channel, "http://twitch.tv/", result["stream"]["created_at"].ToString(), Environment.NewLine));
            reply.Attachments = new List<Attachment>();
            reply.Attachments.Add(previewAttachment);

            return reply;
          }
          else
          {
            return message.CreateReplyMessage(channel + " is offline.");
          }
        }

        return null;
      }
      else
      {
        return HandleSystemMessage(message);
      }
    }

    private Message HandleSystemMessage(Message message)
    {
      if (message.Type == "Ping")
      {
        Message reply = message.CreateReplyMessage();
        reply.Type = "Ping";
        return reply;
      }
      else if (message.Type == "DeleteUserData")
      {
        // Implement user deletion here
        // If we handle user deletion, return a real message
      }
      else if (message.Type == "BotAddedToConversation")
      {
      }
      else if (message.Type == "BotRemovedFromConversation")
      {
      }
      else if (message.Type == "UserAddedToConversation")
      {
      }
      else if (message.Type == "UserRemovedFromConversation")
      {
      }
      else if (message.Type == "EndOfConversation")
      {
      }

      return null;
    }
  }
}
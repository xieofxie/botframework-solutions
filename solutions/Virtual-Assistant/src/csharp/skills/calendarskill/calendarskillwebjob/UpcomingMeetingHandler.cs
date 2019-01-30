using Microsoft.Azure.WebJobs;
using Microsoft.Bot.Connector.DirectLine;
using System;
using System.Threading.Tasks;

public class UpcomingMeetingHandler
{
    [Singleton]
    public static async Task UpcomingMeeting([ServiceBusTrigger("upcomingmeeting")]string message)
    {
        Console.WriteLine($"Queue message: {message}");

        var client = new DirectLineClient(new Uri("https://directline.botframework.com"), new DirectLineClientCredentials("0DIAhAUymSM.cwA.iMI.dGWNsD142-6AvHljA4WXlG4tWS1r4xCnwruDz0a-Jro"));

        var token = await client.Tokens.GenerateTokenForNewConversationAsync(null);
        client = new DirectLineClient(new Uri("https://directline.botframework.com"), new DirectLineClientCredentials(token.Token));

        var conversation = await client.Conversations.StartConversationAsync();

        var activity = Activity.CreateMessageActivity();
        var activityFull = activity as Activity;
        activityFull.From = new ChannelAccount("webjob");
        activityFull.Name = "UpcomingEvent";
        activityFull.Text = "/event:{ 'Name': 'UpcomingEvent', 'Value': {'userId':'284f792c-7028-4034-8958-629030b47135','eventId':'2'} }";

        await client.Conversations.PostActivityAsync(conversation.ConversationId, activityFull).ConfigureAwait(false);

        Console.WriteLine("UpcomingEvent sent to bot");
    }
}
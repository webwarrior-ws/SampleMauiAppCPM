using FirebaseAdmin.Messaging;

namespace GrpcService
{
    public class PushNotificationProvider
    {
        public async Task SendTextPushNotification(int userId, string title, string body)
        {
            Message message = new()
            {
                Topic = userId.ToString(),
                Notification = new FirebaseAdmin.Messaging.Notification()
                {
                    Title = title,
                    Body = body
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

            Console.WriteLine("Successfully sent message: " + response);
        }
    }
}

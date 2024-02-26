using System.Collections.Concurrent;
using System.Threading.Channels;

namespace GrpcService.Services;

public class NotificationProvider
{
    ConcurrentDictionary<int, ChannelWriter<GenericOutputParam>> channels = new ConcurrentDictionary<int, ChannelWriter<GenericOutputParam>>();

    public void AddChannel(int userId, ChannelWriter<GenericOutputParam> channelWriter)
    {
        channels.AddOrUpdate(userId, channelWriter, (_, _) => channelWriter);
    }

    public void RemoveChannel(int userId, ChannelWriter<GenericOutputParam> channelWriter)
    {
        channels.TryRemove(new KeyValuePair<int, ChannelWriter<GenericOutputParam>>(userId, channelWriter));
    }

    public async Task SendNotification(int destUser, string notification)
    {
        if (channels.TryGetValue(destUser, out var notificationChannel))
        {
            await notificationChannel.WriteAsync(
                new GenericOutputParam
                {
                    MsgOut = notification
                }
            );
        }
    }
}
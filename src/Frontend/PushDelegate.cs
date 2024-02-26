#if IOS || ANDROID
using System;
using Shiny.Push;

namespace Frontend
{
    public class PushDelegate : IPushDelegate
    {
        public Task OnEntry(PushNotification data)
        {
            return Task.CompletedTask;
        }

        public Task OnReceived(PushNotification data)
        {
            return Task.CompletedTask;
        }

        public Task OnTokenRefreshed(string token)
        {
            return Task.CompletedTask;
        }

        public async Task OnTokenChanged(string token)
        {
            await Task.CompletedTask;
        }
    }
}
#endif

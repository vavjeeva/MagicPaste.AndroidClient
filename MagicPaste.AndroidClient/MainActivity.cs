using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace MagicPaste.AndroidClient
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            SetupSignalRClient();
        }

        private async void SetupSignalRClient()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("https://magicpaste.azurewebsites.net/MagicPaste")
                .Build();

            connection.On<string>("ReceiveData", (msg) =>
            {
                ShowNotification(msg);
            });

            try
            {
                await connection.StartAsync();
            }
            catch (Exception e)
            {
                Log.Error("MagicPaste", e.Message);
            }
        }


        private void ShowNotification(string msg)
        {
            Notification.Builder builder = new Notification.Builder(this)
                                .SetContentTitle("MagicPaste")
                                .SetContentText(msg)
                                .SetSmallIcon(Resource.Drawable.notification_bg_normal);

            // Build the notification:
            Notification notification = builder.Build();

            // Get the notification manager:
            NotificationManager notificationManager = GetSystemService(NotificationService) as NotificationManager;

            // Publish the notification:
            const int notificationId = 0;
            notificationManager.Notify(notificationId, notification);
        }
    }
}


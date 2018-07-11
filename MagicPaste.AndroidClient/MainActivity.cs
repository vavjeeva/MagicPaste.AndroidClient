using Android.App;
using Android.Widget;
using Android.OS;
using Android.Util;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using Android.Content;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;

namespace MagicPaste.AndroidClient
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : ListActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
         
            ListAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, GetItemsFromPreferences());

            InitializeSignalRClient();
        }
        
        private async void InitializeSignalRClient()
        {            
            var connection = new HubConnectionBuilder()
                .WithUrl("https://magicpaste.azurewebsites.net/MagicPaste")                
                .Build();

            connection.On<string>("ReceiveData", (msg) =>
            {
                SaveData(msg);
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

        private List<string> GetItemsFromPreferences()
        {
            // get shared preferences
            ISharedPreferences pref = Application.Context.GetSharedPreferences("MagicPaste", FileCreationMode.Private);

            // read exisiting value
            var itemsFromSP = pref.GetString("Items", null);

            // if preferences return null, initialize listOfCustomers
            if (itemsFromSP == null)
                return new List<string>();
            
            var items = JsonConvert.DeserializeObject<List<string>>(itemsFromSP);

            if (items == null)
                return new List<string>();

            return items;
        }

        private void SaveData(string msg)
        {
            // get shared preferences
            ISharedPreferences pref = Application.Context.GetSharedPreferences("MagicPaste", FileCreationMode.Private);

            // read exisiting value
            var itemsFromSP = pref.GetString("Items", null);
            IList<string> items;
            // if preferences return null, initialize listOfCustomers
            if (itemsFromSP == null)
                items = new List<string>();
            else
                items = JsonConvert.DeserializeObject<List<string>>(itemsFromSP);

            // add your object to list of customers
            items.Add(msg);

            // convert the list to json
            var itemsAsJson = JsonConvert.SerializeObject(items);

            ISharedPreferencesEditor editor = pref.Edit();

            // set the value to Customers key
            editor.PutString("Items", itemsAsJson);

            // commit the changes
            editor.Commit();            
        }

        private void ShowNotification(string msg)
        {
            Intent intent = new Intent(this, typeof(MainActivity));

            // Create a PendingIntent; we're only using one PendingIntent (ID = 0):
            const int pendingIntentId = 0;
            PendingIntent pendingIntent =
                PendingIntent.GetActivity(this, pendingIntentId, intent, PendingIntentFlags.CancelCurrent);

            Notification.Builder builder = new Notification.Builder(this)
                                .SetContentIntent(pendingIntent)
                                .SetContentTitle("MagicPaste")
                                .SetContentText(msg)
                                .SetAutoCancel(true)
                                .SetSmallIcon(Resource.Drawable.notification_tile_bg);                                

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


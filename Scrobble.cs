using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.UI.Notifications;

namespace MyAppService
{
    public sealed class Scrobble : IBackgroundTask
    {
        private BackgroundTaskDeferral backgroundTaskDeferral;
        private AppServiceConnection appServiceconnection;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            this.backgroundTaskDeferral = taskInstance.GetDeferral(); // Get a deferral so that the service isn't terminated.
            taskInstance.Canceled += OnTaskCanceled; // Associate a cancellation handler with the background task.

            // Retrieve the app service connection and set up a listener for incoming app service requests.
            var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            appServiceconnection = details.AppServiceConnection;
            appServiceconnection.RequestReceived += OnRequestReceived;
        }

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
                var messageDeferral = args.GetDeferral();
                ValueSet returnData = new ValueSet();
            try
            {
                var vault = new PasswordVault();
                var list = vault.FindAllByResource("Lastfmapp");
                var item = list[0];
                item.RetrievePassword();

                ValueSet message = args.Request.Message;

                string song = message["Song"] as string;
                string album = message["Album"] as string;
                string artist = message["Artist"] as string;
                //Lastfm.Services.Session session = new Lastfm.Services.Session("bdcd4cc2b7b85d703370584f5635e84e", "727d4cc441dfca25f82cac2055916e83");
                //session.Authenticate(item.UserName, Lastfm.Utilities.md5(item.Password));
                //Lastfm.Scrobbling.Connection connection = new Lastfm.Scrobbling.Connection("erhhr", "1.3", item.UserName, session);
                //connection.Scrobble(new Lastfm.Scrobbling.Entry());
                KoScrobbler.Scrobbler scrobbler = new KoScrobbler.Scrobbler("bdcd4cc2b7b85d703370584f5635e84e", "727d4cc441dfca25f82cac2055916e83");
                var x = await scrobbler.CreateSessionAsync(item.UserName, item.Password);
                var key = x.SessionKey;
                var session1 = await scrobbler.ValidateSessionAsync(item.UserName, key);
                var blaah = new List<KoScrobbler.Entities.Scrobble>();
                blaah.Add(new KoScrobbler.Entities.Scrobble(artist, album, song, DateTime.Now));
                var scrobble = await scrobbler.ScrobbleAsync(blaah);
                if (scrobble.Success)
                {
                    string toast = $@"<toast>

         <visual>
       
           <binding template='ToastGeneric'>
        
              <text>Last.FM</text>
           
                 <text>Track successfully scrobbled.</text>
              
                    <image placement='appLogoOverride' hint-crop='circle' src='ms-appx:///Assets/checkmark.png'/>
                     
                         </binding>
                     
                       </visual>
                     

                     </toast>";
                    Windows.Data.Xml.Dom.XmlDocument doc = new Windows.Data.Xml.Dom.XmlDocument();
                    doc.LoadXml(toast);
                    var toastNotif = new ToastNotification(doc);


                    // And send the notification
                    ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
                    returnData.Add("Status", "OK");
                }

                try
                {
                    await args.Request.SendResponseAsync(returnData); // Return the data to the caller.
                }
                catch (Exception e)
                {
                    // your exception handling code here
                }
                finally
                {
                    // Complete the deferral so that the platform knows that we're done responding to the app service call.
                    // Note for error handling: this must be called even if SendResponseAsync() throws an exception.
                    messageDeferral.Complete();
                }
            }
            catch (Exception ex)
            {
                string toast = $@"<toast>

         <visual>
       
           <binding template='ToastGeneric'>
        
              <text>Last.FM</text>
           
                 <text>{ex.Message}</text>
              
                    <image placement='appLogoOverride' hint-crop='circle' src='ms-appx:///Assets/xmark.png'/>
                     
                         </binding>
                     
                       </visual>
                     

                     </toast>";
                Windows.Data.Xml.Dom.XmlDocument doc = new Windows.Data.Xml.Dom.XmlDocument();
                doc.LoadXml(toast);
                var toastNotif = new ToastNotification(doc);


                // And send the notification
                ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
                returnData.Add("Status", "Fail: Index out of range");
            }

            try
            {
                await args.Request.SendResponseAsync(returnData); // Return the data to the caller.
            }
            catch (Exception e)
            {
                // your exception handling code here
            }
            finally
            {
                // Complete the deferral so that the platform knows that we're done responding to the app service call.
                // Note for error handling: this must be called even if SendResponseAsync() throws an exception.
                messageDeferral.Complete();
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (this.backgroundTaskDeferral != null)
            {
                // Complete the service deferral.
                this.backgroundTaskDeferral.Complete();
            }
        }
    }
}

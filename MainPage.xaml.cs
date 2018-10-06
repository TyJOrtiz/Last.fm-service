using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace loginDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Lastfm.Services.Session session; //Uses Hqub (https://github.com/avatar29A/Last.fm)
        public MainPage()
        {
            this.InitializeComponent();
        }
        private string ApiKey = ""; //signup for api key at last.fm
        
        private string ApiSecret = ""; //signup for api secret at last.fm
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                session = new Lastfm.Services.Session(ApiKey, ApiSecret);
                session.Authenticate(Userbox.Text, Lastfm.Utilities.md5(Passbox.Password));
                if (SaveLoginCheck.IsChecked == true)
                {
                    var vault = new PasswordVault();

                    var cred = new PasswordCredential("Lastfmapp", Userbox.Text, Passbox.Password);

                    vault.Add(cred);
                }
                var authuser = Lastfm.Services.AuthenticatedUser.GetUser(session);
                string toast = $@"<toast>

         <visual>
       
           <binding template='ToastGeneric'>
        
              <text>Signed in</text>
           
                 <text>You are now signed in as {authuser.Name}</text>
              
                    <image placement='appLogoOverride' hint-crop='circle' src='{authuser.GetImageURL()}'/>
                     
                         </binding>
                     
                       </visual>
                     

                     </toast>";
                Windows.Data.Xml.Dom.XmlDocument doc = new Windows.Data.Xml.Dom.XmlDocument();
                doc.LoadXml(toast);
                var toastNotif = new ToastNotification(doc);


                // And send the notification
                ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
            }
            catch
            {
                Windows.UI.Popups.MessageDialog messageDialog = new Windows.UI.Popups.MessageDialog("Couldn't log in");
                messageDialog.ShowAsync();
            }
        }
    }
}

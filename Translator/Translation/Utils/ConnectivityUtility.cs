using Xamarin.Essentials;

namespace Translation.Utils
{
    public static class ConnectivityUtility
    {
        public static bool IsConnectionAvailable;

        static bool IsFirstLoad = true;

        static void ListenForConnection()
        {
            Connectivity.ConnectivityChanged += (sender, args) =>
            {
                var access = args.NetworkAccess;

                switch (args.NetworkAccess)
                {
                    case NetworkAccess.Internet:
                        IsConnectionAvailable = true;
                        if (!IsFirstLoad)
                        {
                            Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, "Back Online", backgroundColor: "#00c853", position: Acr.UserDialogs.ToastPosition.Top);
                        }
                        break;
                    case NetworkAccess.ConstrainedInternet:
                        IsConnectionAvailable = true;
                        Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, "Limited Connection", position: Acr.UserDialogs.ToastPosition.Top);
                        break;
                    case NetworkAccess.Local:
                    case NetworkAccess.None:
                    case NetworkAccess.Unknown:
                        IsConnectionAvailable = false;
                        if (!IsFirstLoad)
                        {
                            Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, "Connection Lost", position: Acr.UserDialogs.ToastPosition.Top, backgroundColor: "#d50000");
                        }
                        break;
                }
            };
        }

        static bool IsConnected()
        {
            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Internet || current == NetworkAccess.ConstrainedInternet)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void ListenForConnectionChanges()
        {
            if (IsFirstLoad)
            {
                IsConnectionAvailable = IsConnected();
                IsFirstLoad = false;

                if (!IsConnectionAvailable)
                {
                    Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, "No Connection", position: Acr.UserDialogs.ToastPosition.Top, backgroundColor: "#d50000");
                }

                ListenForConnection();
            }
        }
    }
}

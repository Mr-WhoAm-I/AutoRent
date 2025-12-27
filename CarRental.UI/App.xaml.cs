using System.Windows;
using HandyControl.Tools; // Нужно добавить using

namespace CarRental.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConfigHelper.Instance.SetLang("ru");
        }
    }
}
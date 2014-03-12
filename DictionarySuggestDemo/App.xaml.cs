using System.Windows;
using DictionarySuggestDemo.Models;

namespace DictionarySuggestDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            MainWindow = new Window
                {
                    Title = "Dictionary Suggest Demo",
                    Width = 500,
                    Height = 300,
                    FontSize = 18,
                    Content = new ViewModels.SearchViewModel(new WikipediaSuggestService())
                };
            MainWindow.Show();
        }
    }
}

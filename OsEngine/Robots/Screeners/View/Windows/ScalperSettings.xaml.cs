using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OsEngine.Robots.Screeners;

namespace OsEngine.Robots.Screeners
{
    /// <summary>
    /// Логика взаимодействия для ScalperSettings.xaml
    /// </summary>
    public partial class ScalperSettings : Window
    {
        public ScalperSettings(Scalper bot)
        {
            InitializeComponent();
            //DataContext = new SettingsViewModel();
            //DatGridRobots.ItemsSource = bot.Parameters;
            //One_Colum.Binding = new Binding(Convert.ToString(bot.Test));
            //DataContext = bot;
        }
    }
}
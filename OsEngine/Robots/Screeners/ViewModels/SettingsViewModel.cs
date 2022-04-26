using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OsEngine.Robots.Screeners.ViewModels.Base;

namespace OsEngine.Robots.Screeners
{
    // логическая часть отображения приложения
    internal class SettingsViewModel : ViewModel
    {
        #region Заголовок окна

        /// <summary>
        /// Заголовок окна
        /// </summary>
        private string _Title = "Окно настроек";

        public string Title
        {
            get { return _Title; }
            set => Set(ref _Title, value);
        }

        #endregion Заголовок окна
    }
}
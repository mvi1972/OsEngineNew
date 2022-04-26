using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Screeners
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private Phone selectedPhone;

        public ObservableCollection<Phone> Phones { get; set; }

        public Phone SelectedPhone
        {
            get { return selectedPhone; }
            set
            {
                selectedPhone = value;
                СallUpdate("SelectedPhone");
            }
        }

        public SettingsViewModel()
        {
            Phones = new ObservableCollection<Phone>
            {
                new Phone { Title="iPhone 7", Company="Apple", Price=56000 },
                new Phone {Title="Galaxy S7 Edge", Company="Samsung", Price =60000 },
                new Phone {Title="Elite x3", Company="HP", Price=56000 },
                new Phone {Title="Mi5S", Company="Xiaomi", Price=35000 }
            };
        }

        // дальше реализация INotifyPropertyChanged
        /// <summary>
        /// обработчик события изменения свойств
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void СallUpdate(string name)  // сигнализирует об изменении свойств
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        ///  сверяет значения данных и выдает сигнал об изменении
        /// </summary>
        protected void Set<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!field.Equals(value))
            {
                field = value;
                СallUpdate(name);
            }
        }
    }
}
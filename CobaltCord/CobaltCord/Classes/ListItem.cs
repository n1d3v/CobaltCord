using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CobaltCord.Classes
{
    public class ListItem : INotifyPropertyChanged
    {
        private string _secondaryText;

        public string Name { get; set; }
        public string CombinedId { get; set; }

        public string SecondaryText
        {
            get { return _secondaryText; }
            set
            {
                if (_secondaryText != value)
                {
                    _secondaryText = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
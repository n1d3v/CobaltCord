using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CobaltCord.Classes
{
    public class ListItem : INotifyPropertyChanged
    {
        private string _itemName;
        private string _itemSecondaryText;
        private string _messageTime;
        private int _notificationCount;

        public string ItemName
        {
            get { return _itemName; }
            set
            {
                if (_itemName != value)
                {
                    _itemName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ItemSecondaryText
        {
            get { return _itemSecondaryText; }
            set
            {
                if (_itemSecondaryText != value)
                {
                    _itemSecondaryText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MessageTime
        {
            get { return _messageTime; }
            set
            {
                if (_messageTime != value)
                {
                    _messageTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public int NotificationCount
        {
            get { return _notificationCount; }
            set
            {
                if (_notificationCount != value)
                {
                    _notificationCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CombinedId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
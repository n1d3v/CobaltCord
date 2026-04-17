using System.ComponentModel;

namespace CobaltCord.Classes
{
    public class ListMsgItem
    {
        public string AuthorName { get; set; }
        public string AuthorId { get; set; }
        public string AuthorHash { get; set; }

        public string MessageId { get; set; }
        public string MessageTime { get; set; }
        public string MessageText { get; set; }

        public bool IsContinuation { get; set; }

        private bool _isLastContinuation;
        public bool IsLastContinuation
        {
            get { return _isLastContinuation; }
            set
            {
                _isLastContinuation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLastContinuation)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
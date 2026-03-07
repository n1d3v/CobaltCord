using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CobaltCord.Classes;
using System.Threading.Tasks;

namespace CobaltCord.UserControls
{
    public sealed partial class SenderMsgItem : UserControl
    {
        public SenderMsgItem()
        {
            this.InitializeComponent();
        }

        private async void ReloadAvatar()
        {
            string id = AuthorId;
            string hash = AuthorHash;

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(hash))
                return;

            string avatarUrl = HelperMethods.GetAvatarUrl(id, hash, false, false);
            await AvatarHelper.SetAvatarFromHash(UserAvatar, id, hash, avatarUrl);
            if (AuthorId != id || AuthorHash != hash)
            {
                UserAvatar.Source = null;
            }
        }

        public string AuthorName
        {
            get { return (string)GetValue(AuthorNameProperty); }
            set { SetValue(AuthorNameProperty, value); }
        }

        public static readonly DependencyProperty AuthorNameProperty =
            DependencyProperty.Register(
                nameof(AuthorName),
                typeof(string),
                typeof(SenderMsgItem),
                new PropertyMetadata(string.Empty));

        public string AuthorId
        {
            get { return (string)GetValue(AuthorIdProperty); }
            set { SetValue(AuthorIdProperty, value); }
        }

        public static readonly DependencyProperty AuthorIdProperty =
            DependencyProperty.Register(
                nameof(AuthorId),
                typeof(string),
                typeof(SenderMsgItem),
                new PropertyMetadata(string.Empty, OnAuthorIdChanged));

        private static void OnAuthorIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = d as SenderMsgItem;
            if (item != null)
                item.ReloadAvatar();
        }

        public string AuthorHash
        {
            get { return (string)GetValue(AuthorHashProperty); }
            set { SetValue(AuthorHashProperty, value); }
        }

        public static readonly DependencyProperty AuthorHashProperty =
            DependencyProperty.Register(
                nameof(AuthorHash),
                typeof(string),
                typeof(SenderMsgItem),
                new PropertyMetadata(string.Empty));

        public string MessageTime
        {
            get { return (string)GetValue(MessageTimeProperty); }
            set { SetValue(MessageTimeProperty, value); }
        }

        public static readonly DependencyProperty MessageTimeProperty =
            DependencyProperty.Register(
                nameof(MessageTime),
                typeof(string),
                typeof(SenderMsgItem),
                new PropertyMetadata(string.Empty));

        public string MessageText
        {
            get { return (string)GetValue(MessageTextProperty); }
            set { SetValue(MessageTextProperty, value); }
        }

        public static readonly DependencyProperty MessageTextProperty =
            DependencyProperty.Register(
                nameof(MessageText),
                typeof(string),
                typeof(SenderMsgItem),
                new PropertyMetadata(string.Empty));

        public string MessageId
        {
            get { return (string)GetValue(MessageIdProperty); }
            set { SetValue(MessageIdProperty, value); }
        }

        public static readonly DependencyProperty MessageIdProperty =
            DependencyProperty.Register(
                nameof(MessageId),
                typeof(string),
                typeof(SenderMsgItem),
                new PropertyMetadata(string.Empty));
    }
}
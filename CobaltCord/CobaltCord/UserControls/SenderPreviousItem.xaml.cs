using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CobaltCord.UserControls
{
    public sealed partial class SenderPreviousItem : UserControl
    {
        public SenderPreviousItem()
        {
            this.InitializeComponent();
        }

        public string MessageText
        {
            get { return (string)GetValue(MessageTextProperty); }
            set { SetValue(MessageTextProperty, value); }
        }

        public static readonly DependencyProperty MessageTextProperty =
            DependencyProperty.Register(
                nameof(MessageText),
                typeof(string),
                typeof(SenderPreviousItem),
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
                typeof(SenderPreviousItem),
                new PropertyMetadata(string.Empty));

        public bool IsLastContinuation
        {
            get { return (bool)GetValue(IsLastContinuationProperty); }
            set { SetValue(IsLastContinuationProperty, value); }
        }

        public static readonly DependencyProperty IsLastContinuationProperty =
            DependencyProperty.Register(
                nameof(IsLastContinuation),
                typeof(bool),
                typeof(SenderPreviousItem),
                new PropertyMetadata(false, OnIsLastContinuationChanged));

        private static void OnIsLastContinuationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = d as SenderPreviousItem;
            item.MessageTextBlock.Margin = (bool)e.NewValue
                ? new Thickness(65, 1.5, 0, 0)
                : new Thickness(65, 1.5, 0, 1.5);
        }
    }
}
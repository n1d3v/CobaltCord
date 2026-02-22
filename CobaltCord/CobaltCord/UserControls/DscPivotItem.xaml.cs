using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CobaltCord.UserControls
{
    public sealed partial class DscPivotItem : UserControl
    {
        public DscPivotItem()
        {
            this.InitializeComponent();
        }

        public string ItemName
        {
            get { return (string)GetValue(ItemNameProperty); }
            set { SetValue(ItemNameProperty, value); }
        }

        public static readonly DependencyProperty ItemNameProperty =
            DependencyProperty.Register(
                nameof(ItemName),
                typeof(string),
                typeof(DscPivotItem),
                new PropertyMetadata(string.Empty));

        public string ItemSecondaryText
        {
            get { return (string)GetValue(ItemSecondaryTextProperty); }
            set { SetValue(ItemSecondaryTextProperty, value); }
        }

        public static readonly DependencyProperty ItemSecondaryTextProperty =
            DependencyProperty.Register(
                nameof(ItemSecondaryText),
                typeof(string),
                typeof(DscPivotItem),
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
                typeof(DscPivotItem),
                new PropertyMetadata(string.Empty));

        public string CombinedId
        {
            get { return (string)GetValue(CombinedIdProperty); }
            set { SetValue(CombinedIdProperty, value); }
        }

        public static readonly DependencyProperty CombinedIdProperty =
            DependencyProperty.Register(
                nameof(CombinedId),
                typeof(string),
                typeof(DscPivotItem),
                new PropertyMetadata(string.Empty));
    }
}
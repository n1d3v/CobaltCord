using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CobaltCord.UserControls;

namespace CobaltCord.Classes
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SenderMsgTemplate { get; set; }
        public DataTemplate SenderPreviousTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var msg = item as ListMsgItem;
            if (msg != null && msg.IsContinuation)
                return SenderPreviousTemplate;
            return SenderMsgTemplate;
        }
    }
}
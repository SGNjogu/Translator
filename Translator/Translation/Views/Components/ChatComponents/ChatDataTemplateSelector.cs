using Xamarin.Forms;

namespace Translation.Views.Components.ChatComponents
{
    public class ChatDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Person1 { get; set; }
        public DataTemplate Person2 { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if(item != null)
            {
                if (((Models.TranslationResultText)item).IsPerson1)
                    return Person1;
                return Person2;
            }
            return null;
        }
    }
}

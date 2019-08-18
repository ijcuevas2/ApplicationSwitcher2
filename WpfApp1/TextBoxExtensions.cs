using System;
using System.Windows;
using System.Windows.Controls;

namespace ApplicationSwitcher
{

    public class TextBoxExtensions : DependencyObject 
    {
        public static readonly DependencyProperty NonIntrusiveTextProperty = DependencyProperty.RegisterAttached("NonIntrusiveText"
                              , typeof(string)
                              , typeof(TextBoxExtensions)
                              , new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                                  NonIntrusiveTextChanged));

        public static string GetNonIntrusiveText(DependencyObject obj)
        {
            return (string)obj.GetValue(NonIntrusiveTextProperty);
        }

        public static void SetNonIntrusiveText(DependencyObject obj, string value)
        {
            obj.SetValue(NonIntrusiveTextProperty, value);
        }

        public static void NonIntrusiveTextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }

            var caretIndex = textBox.CaretIndex;
            var selectionStart = textBox.SelectionStart;
            var selectionLength = textBox.SelectionLength;
            textBox.Text = (string)e.NewValue;
            textBox.CaretIndex = caretIndex;
            textBox.SelectionStart = selectionStart;
            textBox.SelectionLength = selectionLength;
        }
    }
}

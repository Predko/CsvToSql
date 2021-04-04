using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CSVtoSQL
{
    class WaterMarkForTextBox
    {
        private class WaterMarkElement
        {
            public TextBox textBox;
            public string waterMark;
            public object obj;

            public Color colorWm;
            public Color colorTb;

            public WaterMarkElement(TextBox tb, string wm, object o, Color color)
            {
                textBox = tb;
                waterMark = wm;
                obj = o;

                colorTb = ((SolidColorBrush)tb.Background).Color;
                colorWm = color;
            }
        }

        /// <summary>
        /// Словарь, хранящий строки "водяных знаков" текстовых полей и признак множественного выбора файлов.
        /// </summary>
        private readonly Dictionary<TextBox, WaterMarkElement> emptyLinesOfTextBoxs = new Dictionary<TextBox, WaterMarkElement>();

        public void AddWaterMarkString(TextBox tb, string es, Color colorWm, object obj = null)
        {
            tb.Foreground = Brushes.Gray;

            tb.Text = es;

            emptyLinesOfTextBoxs.Add(tb, new WaterMarkElement(tb, es, obj, colorWm));
        }

        public void RemoveWaterMarkString(TextBox tb) => emptyLinesOfTextBoxs.Remove(tb);

        public string GetWmString(TextBox tb) => emptyLinesOfTextBoxs[tb].waterMark;

        public void SetWmString(TextBox tb, string wm) => emptyLinesOfTextBoxs[tb].waterMark = wm;

        public Color GetWmColor(TextBox tb) => emptyLinesOfTextBoxs[tb].colorWm;

        public void SetWmColor(TextBox tb, Color colorWm) => emptyLinesOfTextBoxs[tb].colorWm = colorWm;

        public object GetObject(TextBox tb) => emptyLinesOfTextBoxs[tb].obj;
        
        public void SetObject(TextBox tb, object obj) => emptyLinesOfTextBoxs[tb].obj = obj;

        public bool IsWaterMarkTextBoxEmpty(TextBox tb) => (emptyLinesOfTextBoxs[tb].waterMark == tb.Text);


        #region События для обработки водяного знака
        /// <summary>
        /// Получение фокуса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TbWm_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!(sender is TextBox tb))
            {
                return;
            }

            //If nothing has been entered yet.
            if (tb.Foreground == Brushes.Gray)
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
            }
        }

        /// <summary>
        /// Потеря фокуса.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TbWm_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!(sender is TextBox tb))
            {
                return;
            }

            //If nothing was entered, reset default text.
            if (tb.Text.Trim().Length == 0)
            {
                tb.Foreground = Brushes.Gray;

                tb.Text = GetWmString(tb);
            }
        }
        #endregion

    }
}

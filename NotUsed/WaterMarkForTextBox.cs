using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CSVtoDataBase
{
    /// <summary>
    /// Класс, обеспечивающий создание водяного знака в текстовом поле.
    /// Позволяет, добавить и удалить водяной знак, определить есть ли в поле запись
    /// и добавить или получить некий объект, соответствующий данному текстовому полю.
    /// </summary>
    public class WaterMarkForTextBox
    {
        /// <summary>
        /// Хранит данные для создания водяного знака для TextBox.
        /// Также обеспечивает хранение элемента данных типа Object, кисть водяного знака и строку.
        /// </summary>
        private class WaterMarkElement
        {
            public TextBox textBox;
            public string waterMark;
            public object obj;

            public Brush brushWm;
            public Brush brushTb;

            public WaterMarkElement(TextBox tb, string wm, object o, Brush brush)
            {
                textBox = tb;
                waterMark = wm;
                obj = o;

                brushTb = tb.Foreground;
                brushWm = brush;

                tb.Foreground = brushWm;
                tb.Text = wm;
            }


        }

        /// <summary>
        /// Словарь, хранящий строки "водяных знаков" текстовых полей и признак множественного выбора файлов.
        /// </summary>
        private readonly Dictionary<TextBox, WaterMarkElement> emptyLinesOfTextBoxs = new Dictionary<TextBox, WaterMarkElement>();

        public void AddWaterMark(TextBox tb, string es, Brush brushWm, object obj = null)
        {
            emptyLinesOfTextBoxs.Add(tb, new WaterMarkElement(tb, es, obj, brushWm));

            tb.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(TbWm_GotKeyboardFocus);

            tb.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(TbWm_LostKeyboardFocus);
        }

        public bool IsContains(TextBox tb) => emptyLinesOfTextBoxs.ContainsKey(tb);

        public void RemoveWaterMark(TextBox tb) => emptyLinesOfTextBoxs.Remove(tb);

        public string GetWmString(TextBox tb) => emptyLinesOfTextBoxs[tb].waterMark;

        public void SetWmString(TextBox tb, string wm) => emptyLinesOfTextBoxs[tb].waterMark = wm;

        public Brush GetTbBrush(TextBox tb) => emptyLinesOfTextBoxs[tb].brushTb;

        public void SetTbBrush(TextBox tb, Brush brushTb) => emptyLinesOfTextBoxs[tb].brushTb = brushTb;

        public Brush GetWmBrush(TextBox tb) => emptyLinesOfTextBoxs[tb].brushWm;

        public void SetWmBrush(TextBox tb, Brush brushWm) => emptyLinesOfTextBoxs[tb].brushWm = brushWm;

        public object GetObject(TextBox tb) => emptyLinesOfTextBoxs[tb].obj;

        public void SetObject(TextBox tb, object obj) => emptyLinesOfTextBoxs[tb].obj = obj;

        public bool WaterMarkTextBoxIsEmpty(TextBox tb) => (emptyLinesOfTextBoxs[tb].waterMark == tb.Text);


        #region События для обработки водяного знака
        /// <summary>
        /// Получение фокуса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TbWm_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!(sender is TextBox tb) || emptyLinesOfTextBoxs.ContainsKey(tb) == false)
            {
                return;
            }

            //If nothing has been entered yet.
            if (tb.Foreground == GetWmBrush(tb))
            {
                tb.Text = "";
                tb.Foreground = GetTbBrush(tb);
            }
        }

        /// <summary>
        /// Потеря фокуса.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TbWm_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!(sender is TextBox tb) || emptyLinesOfTextBoxs.ContainsKey(tb) == false)
            {
                return;
            }

            //If nothing was entered, reset default text.
            if (tb.Text.Trim().Length == 0)
            {
                tb.Foreground = GetWmBrush(tb);

                tb.Text = GetWmString(tb);
            }
        }
        #endregion

    }
}

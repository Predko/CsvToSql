using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CSVtoSQL
{
    public class MessageForEmptyTextBox
    {
        private readonly TextBox textBox;

        /// <summary>
        /// Приращение цвета фона.
        /// </summary>
        private int deltaR = 5;

        /// <summary>
        /// Цвет фона текст бокса.
        /// </summary>
        private Color oldColor;

        /// <summary>
        /// Максимальное изменение цвета
        /// </summary>
        private byte maxDeltaColor = 150;

        private byte Rmin, Rmax;

        public MessageForEmptyTextBox(TextBox tb)
        {
            textBox = tb;
        }


        /// <summary>
        /// Выводит сообщение и циклически меняет цвет поля ввода.
        /// </summary>
        public void Show(string message)
        {
            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 10),
            };

            timer.Tick += Timer_tick;

            oldColor = ((SolidColorBrush)textBox.Background).Color;

            int maxR = oldColor.R + maxDeltaColor;

            Rmax = (maxR > 255) ? (byte)255 : (byte)(maxR);

            Rmin = (byte)(Rmax - maxDeltaColor);

            textBox.Background = new SolidColorBrush(Color.FromArgb(oldColor.A, oldColor.R, oldColor.G, oldColor.B));
            timer.Start();

            MessageBox.Show(message);

            timer.Stop();

            ((SolidColorBrush)textBox.Background).Color = oldColor;
        }

        /// <summary>
        /// Каждый тик таймера циклически меняет прозрачность фона tbFileCSV
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_tick(object sender, EventArgs e)
        {
            Color color = ((SolidColorBrush)textBox.Background).Color;

            int newR = color.R + deltaR;

            if (deltaR > 0 && newR >= Rmax)
            {
                newR = Rmax;

                deltaR = -deltaR;
            }
            else
            if (deltaR < 0 && newR <= Rmin)
            {
                newR = Rmin;

                deltaR = -deltaR;
            }

            color.R = (byte)newR;

            ((SolidColorBrush)textBox.Background).Color = color;
        }

    }
}

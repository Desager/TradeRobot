using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TradeRobot
{
    public partial class MainWindow : Window
    {
        private int chartInterval;
        private XmlManager currencies;
        private XmlManager rate;
        private Robot robot;

        public MainWindow()
        {
            chartInterval = -1;

            currencies = new XmlManager();
            rate = new XmlManager();
            robot = new Robot();

            //Подгрузка всех валют
            try
            {
                currencies.Load("http://www.cbr.ru/scripts/XML_daily.asp");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            InitializeComponent();
        }

        //Занести все названия валют в выпадающий список
        private void CurrencyComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (!currencies.IsLoaded)
                return;

            ComboBox comboBox = (ComboBox)sender;

            for (int i = 0; i < currencies.Count; i++)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = currencies.GetName(i);

                comboBox.Items.Add(item);
            }
        }

        private async void CurrencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRate();
            DrawChart();

            int index = CurrencyComboBox.SelectedIndex;
            Task<string> adviceAsync = robot.getForecast(currencies.GetCharCode(index), currencies.GetName(index));
            string advice = await adviceAsync;
            chatStackPanel.Children.Insert(0, createMessage(advice));
        }

        //Изменение интервала для котировки (значение хранится в теге у кнопки)
        private void IntervalButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int interval = int.Parse(button.Tag.ToString());

            if (interval != chartInterval)
            {
                chartInterval = interval;
                UpdateRate();
                DrawChart();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawChart();
        }

        //Подгрузка котировки валюты
        private void UpdateRate()
        {
            try
            {
                rate.Load("http://www.cbr.ru/scripts/XML_dynamic.asp?" +
                    "date_req1=" + DateTime.Now.AddMonths(chartInterval).ToShortDateString() +
                    "&date_req2=" + DateTime.Now.ToShortDateString() +
                    "&VAL_NM_RQ=" + currencies.GetId(CurrencyComboBox.SelectedIndex));
            }
            catch { }
        }

        private TextBlock createMessage(string text)
        {
            TextBlock message = new TextBlock();

            message.Text = text;
            message.Background = Brushes.WhiteSmoke;
            message.Margin = new Thickness(5);
            message.Padding = new Thickness(5);
            message.TextWrapping = TextWrapping.Wrap;

            return message;
        }

        private void DrawChart()
        {
            if (!rate.IsLoaded)
                return;

            ChartCanvas.Children.Clear();

            double min = FindMin();
            double max = FindMax();
            double XPadding = 10;
            double YPadding = 20;
            double XOffset = (ChartCanvas.ActualWidth - XPadding * 2) / (rate.Count - 1);
            double YOffset = (ChartCanvas.ActualHeight - YPadding * 2) / (max - min);

            if (YPadding * 2 >= ChartCanvas.ActualHeight ||
                XPadding * 2 >= ChartCanvas.ActualWidth)
                return;

            for (int i = 1; i < rate.Count; i++)
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;

                //Чтобы график был равномерный делим на номинал
                line.X1 = XPadding + (i - 1) * XOffset;
                line.Y1 = ChartCanvas.ActualHeight - YPadding - (rate.GetValue(i - 1)
                            / rate.GetNominal(i - 1) - min) * YOffset;

                line.X2 = XPadding + i * XOffset;
                line.Y2 = ChartCanvas.ActualHeight - YPadding - (rate.GetValue(i)
                            / rate.GetNominal(i) - min) * YOffset;

                ChartCanvas.Children.Add(line);

                //Рисование вертикальной линии на значении
                //У линии маленький хитбокс, поэтому рисуем собственный
                Line valueLine = new Line();
                Rectangle valueLineHitbox = new Rectangle();
                valueLine.X1 = line.X1; valueLine.X2 = line.X1;
                valueLine.Y1 = 0; valueLine.Y2 = ChartCanvas.ActualHeight;
                valueLine.StrokeDashArray = new DoubleCollection() { 10, 10 }; ;
                valueLine.Stroke = Brushes.LimeGreen;
                valueLine.Opacity = 0;
                valueLine.Tag = i - 1;

                //На левом крае размеры хитбокса отличаются
                if (i == 1)
                {
                    valueLineHitbox.Margin = new Thickness(
                        valueLine.X1 - XOffset / 2 - XPadding, 0, 0, 0
                    );
                    valueLineHitbox.Width = XOffset + XPadding;
                }
                else
                {
                    valueLineHitbox.Margin = new Thickness(
                        valueLine.X1 - XOffset / 2, 0, 0, 0
                    );
                    valueLineHitbox.Width = XOffset;
                }
                valueLineHitbox.Height = ChartCanvas.ActualHeight;
                valueLineHitbox.Fill = Brushes.Transparent;
                valueLineHitbox.Tag = valueLine;
                valueLineHitbox.MouseEnter += showHighlight;
                valueLineHitbox.MouseLeave += hideHighlight;
                valueLineHitbox.MouseMove += hitboxMouseMoved;
                ChartCanvas.Children.Add(valueLine);
                ChartCanvas.Children.Add(valueLineHitbox);
            }

            //Рисование крайней правой линии, потому что в цикл она не включена
            Line valueLineEnd = new Line();
            Rectangle valueLineHitboxEnd = new Rectangle();
            double x = XPadding + (rate.Count - 1) * XOffset;
            valueLineEnd.X1 = x; valueLineEnd.X2 = x;
            valueLineEnd.Y1 = 0; valueLineEnd.Y2 = ChartCanvas.ActualHeight;
            valueLineEnd.StrokeDashArray = new DoubleCollection() { 10, 10 };
            valueLineEnd.Stroke = Brushes.LimeGreen;
            valueLineEnd.Opacity = 0;
            valueLineEnd.Tag = rate.Count - 1;

            valueLineHitboxEnd.Margin = new Thickness(
                valueLineEnd.X1 - XOffset / 2, 0, 0, 0
            );
            valueLineHitboxEnd.Width = XOffset / 2 + XPadding;
            valueLineHitboxEnd.Height = ChartCanvas.ActualHeight;
            valueLineHitboxEnd.Fill = Brushes.Transparent;
            valueLineHitboxEnd.Tag = valueLineEnd;
            valueLineHitboxEnd.MouseEnter += showHighlight;
            valueLineHitboxEnd.MouseLeave += hideHighlight;
            valueLineHitboxEnd.MouseMove += hitboxMouseMoved;
            ChartCanvas.Children.Add(valueLineEnd);
            ChartCanvas.Children.Add(valueLineHitboxEnd);
        }

        private void hitboxMouseMoved(object sender, MouseEventArgs e)
        {
            //Сначала вызывается ивент MouseEnter
            //поэтому сразу знаем, что showHightlight уже один раз был вызван
            //и проверка на это не требуется
            hideHighlight(sender, e);
            showHighlight(sender, e);
        }

        //Подсветка и отображение текста при наведении на хитбокс
        private void showHighlight(object sender, MouseEventArgs e)
        {
            Rectangle hitbox = (Rectangle)sender;

            Line valueLine = (Line)hitbox.Tag;
            valueLine.Opacity = 1;

            Label label = new Label();
            int index = (int)valueLine.Tag;

            // DD.MM.YYYY
            // 1 CUR = .. RUB
            label.Content = rate.GetDate(index) + "\n" +
                "1 " + currencies.GetCharCode(CurrencyComboBox.SelectedIndex) +
                " = " + rate.GetValue(index) / rate.GetNominal(index) + " RUB";

            Point p = e.GetPosition(ChartCanvas);
            double YMargin = 15;
            label.Margin = new Thickness(valueLine.X1, p.Y + YMargin, 0, 0);
            label.IsHitTestVisible = false;
            label.Background = Brushes.White;
            label.BorderThickness = new Thickness(1);
            label.BorderBrush = Brushes.Black;

            ChartCanvas.Children.Add(label);
        }

        //Удаление подсветки и текста
        private void hideHighlight(object sender, MouseEventArgs e)
        {
            Rectangle hitbox = (Rectangle)sender;
            Line line = (Line)hitbox.Tag;
            line.Opacity = 0;

            //Текст добавляется последним, поэтому удаляем последний элемент
            ChartCanvas.Children.RemoveAt(ChartCanvas.Children.Count - 1);
        }

        private double FindMax()
        {
            double max = double.MinValue;

            for (int i = 0; i < rate.Count; i++)
            {
                double value = rate.GetValue(i) / rate.GetNominal(i);

                max = max < value ? value : max;
            }

            return max;
        }

        private double FindMin()
        {
            double min = double.MaxValue;

            for (int i = 0; i < rate.Count; i++)
            {
                double value = rate.GetValue(i) / rate.GetNominal(i);

                min = min > value ? value : min;
            }

            return min;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Windows.Controls;
using Microsoft.Windows.Controls.Primitives;
using AutoBlankMapBuilder.Models;

namespace AutoBlankMapBuilder.Views
{
    /// <summary>
    /// AlarmView.xaml の相互作用ロジック
    /// </summary>
    public partial class AlarmView : Window
    {
        private List<AlarmInfo> Alarms;

        public AlarmView(List<AlarmInfo> alarms)
        {
            InitializeComponent();
            DataGrid dataGrid = new DataGrid();
            _grid.Children.Add(dataGrid);

            Alarms = alarms;

            // 列ヘッダのスタイル
            Style style = new Style();

            style.Setters.Add(new Setter(DataGridColumnHeader.BackgroundProperty,
                new LinearGradientBrush(Colors.Blue, Colors.Navy, 90)));
            style.Setters.Add(new Setter(DataGridColumnHeader.ForegroundProperty, new SolidColorBrush(Colors.White)));

            style.Setters.Add(new Setter(DataGridColumnHeader.BorderBrushProperty, new SolidColorBrush(Colors.White)));
            style.Setters.Add(new Setter(DataGridColumnHeader.BorderThicknessProperty, new Thickness(1)));

            dataGrid.ColumnHeaderStyle = style;

            dataGrid.Columns.Add(new DataGridTextColumn() {Header = "処理時間", Binding = new Binding("Time")});
            dataGrid.Columns.Add(new DataGridTextColumn() {Header = "部署コード", Binding = new Binding("Department")});
            dataGrid.Columns.Add(new DataGridTextColumn() {Header = "オーダNO", Binding = new Binding("OrderNo")});
            dataGrid.Columns.Add(new DataGridTextColumn() {Header = "KISYU", Binding = new Binding("Model")});
            dataGrid.Columns.Add(new DataGridTextColumn() {Header = "投入日", Binding = new Binding("StartDate")});
            dataGrid.Columns.Add(new DataGridTextColumn() {Header = "投入数量主", Binding = new Binding("Quantity")});
            dataGrid.Columns.Add(new DataGridTextColumn() {Header = "処理結果", Binding = new Binding("Result")});

            dataGrid.AlternatingRowBackground = new SolidColorBrush(Colors.AliceBlue);

            dataGrid.AutoGenerateColumns = false;

            dataGrid.CanUserAddRows = true;

            dataGrid.CanUserDeleteRows = true;

            dataGrid.Height = 300;

            dataGrid.DataContext = Alarms;
            dataGrid.SetBinding(DataGrid.ItemsSourceProperty, new Binding());
        }
    }
}


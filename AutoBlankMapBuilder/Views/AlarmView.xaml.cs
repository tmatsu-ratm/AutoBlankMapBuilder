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

namespace AutoBlankMapBuilder.Views
{
    /// <summary>
    /// AlarmView.xaml の相互作用ロジック
    /// </summary>
    public partial class AlarmView : Window
    {
        public AlarmView()
        {
            InitializeComponent();
            DataGrid dataGrid = new DataGrid();
            _grid.Children.Add(dataGrid);

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

            dataGrid.DataContext = new List<AlarmInfo>()
            {
                new AlarmInfo()
                {
                    Time = DateTime.Now, Department = "T661", OrderNo = "9AM8T-3631", Model = "BU3A251MN-8GBQY",
                    StartDate = "20190605", Quantity = 25, Result = "ネットワークエラー"
                },
                new AlarmInfo()
                {
                    Time = DateTime.Now, Department = "T661", OrderNo = "9AM8T-3632", Model = "BUWT2416AX-8GTY",
                    StartDate = "20190605", Quantity = 25, Result = "ブランクMAP登録なし"
                },
            };
            dataGrid.SetBinding(DataGrid.ItemsSourceProperty, new Binding());
        }
    }

    public class AlarmInfo
    {
        public DateTime Time { get; set; }
        public string Department { get; set; }
        public string OrderNo { get; set; }
        public string Model { get; set; }
        public string StartDate { get; set; }
        public int Quantity { get; set; }
        public string Result { get; set; }
    }
}


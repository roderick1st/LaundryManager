using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Shapes;

namespace LaundryManager
{
    /// <summary>
    /// Interaction logic for XeroImportIssue.xaml
    /// </summary>
    public partial class XeroImportIssue : Window
    {

        DataTable glob_TicketIssues = new();

        public XeroImportIssue(DataTable issuesTable)
        {
            InitializeComponent();
            glob_TicketIssues = issuesTable;
        }

        private void XeroImportIssue_Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataView issuesGrid = new(glob_TicketIssues);
            dataGridReport.ItemsSource = issuesGrid;

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

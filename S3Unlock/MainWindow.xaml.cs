/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Data;
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

namespace S3Unlock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SqlCmd sql;
        
        /// <summary>
        /// MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            sql = new SqlCmd();
            versionLabel.Content = "Version: " + Version;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (sql.CreateConnection(this.User.Text, this.Pass.Password, this.Server.Text, this.Database.Text))
            {
                this.Connect.IsEnabled = false;
                GetLockAgents();
            }
            else
            {
                MessageBox.Show("Try again, can't get connected :(", "Error connecting...");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sql.UpdateAll)
                    GetLockAgents();
                else
                    MessageBox.Show("Failed");
            }
            catch (Exception)
            {
                MessageBox.Show("Failed");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            int cnt = dataGrid.SelectedItems.Count;
            for (int i = 0; i < cnt; i++)
            {
                DataRowView row = (DataRowView)dataGrid.SelectedItems[i];
                try
                {
                    if (sql.UpdateSingle(row["agent_guid"].ToString()))
                        GetLockAgents();
                    else
                        MessageBox.Show("Failed");
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed");
                }
            }
        }

        private void GetLockAgents()
        {
            DataTable lockAgents = sql.GetS3LockedAgents;
            if (lockAgents.Rows.Count > 0)
            {
                this.dataGrid.DataContext = lockAgents;
                this.Smiley.Visibility = Visibility.Hidden;
                this.dataGrid.Visibility = Visibility.Visible;
                this.Reset.Visibility = Visibility.Visible;
                this.ResetAll.Visibility = Visibility.Visible;
            }
            else
            {
                this.Smiley.Visibility = Visibility.Visible;
                this.dataGrid.Visibility = Visibility.Hidden;
                this.Reset.Visibility = Visibility.Hidden;
                this.ResetAll.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Gets the application version
        /// </summary>
        protected internal static string Version
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }
    }
}

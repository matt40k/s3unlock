/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Matt40k.S3Unlock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SqlCmd sql;
        private bool IsSqlAuth = true;
        
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
            if (IsSqlAuth)
            {
                // Use SQL Authentication
                if (sql.CreateConnection(this.User.Text, this.Pass.Password, this.Server.Text, this.Database.Text))
                {
                    this.Connect.IsEnabled = false;
                    GetLockAgents();
                    GetLockDeployments();
                }
                else
                {
                    MessageBox.Show("Try again, can't get connected :(", "Error connecting...");
                }
            }
            else
            {
                // Use Windows Authentication
                if (sql.CreateConnection(this.Server.Text, this.Database.Text))
                {
                    this.Connect.IsEnabled = false;
                    GetLockAgents();
                    GetLockDeployments();
                }
                else
                {
                    MessageBox.Show("Try again, can't get connected :(", "Error connecting...");
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Selected tab: " + (tc.SelectedItem as TabItem).Header);
            try
            {
                if (sql.UpdateAllS3AgentStatus)
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
            /*
            int cnt = dataGrid.SelectedItems.Count;
            for (int i = 0; i < cnt; i++)
            {
                DataRowView row = (DataRowView)dataGrid.SelectedItems[i];
                try
                {
                    if (sql.UpdateSingleS3AgentStatus(row["agent_guid"].ToString()))
                        GetLockAgents();
                    else
                        MessageBox.Show("Failed");
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed");
                }
            }*/
        }

        /// <summary>
        ///  
        /// </summary>
        private void GetLockAgents()
        {
            DataTable lockAgents = sql.GetS3LockedAgents;
            if (lockAgents.Rows.Count > 0)
            {
                this.dataGridAgent.DataContext = lockAgents;
                this.SmileyAgent.Visibility = Visibility.Hidden;
                this.dataGridAgent.Visibility = Visibility.Visible;
                this.ResetAgent.Visibility = Visibility.Visible;
                this.ResetAllAgent.Visibility = Visibility.Visible;
            }
            else
            {
                this.SmileyAgent.Visibility = Visibility.Visible;
                this.dataGridAgent.Visibility = Visibility.Hidden;
                this.ResetAgent.Visibility = Visibility.Hidden;
                this.ResetAllAgent.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetLockDeployments()
        {
            DataTable lockDeployments = sql.GetS3LockedDeployments;
            if (lockDeployments.Rows.Count > 0)
            {
                this.dataGridDeployment.DataContext = lockDeployments;
                this.SmileyDeployment.Visibility = Visibility.Hidden;
                this.dataGridDeployment.Visibility = Visibility.Visible;
                this.ResetDeployment.Visibility = Visibility.Visible;
                this.ResetAllDeployment.Visibility = Visibility.Visible;
            }
            else
            {
                this.SmileyDeployment.Visibility = Visibility.Visible;
                this.dataGridDeployment.Visibility = Visibility.Hidden;
                this.ResetDeployment.Visibility = Visibility.Hidden;
                this.ResetAllDeployment.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Gets the application version
        /// </summary>
        protected internal static string Version
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        /// <summary>
        /// Set the SQL authentication
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AuthRadioButton_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var value = (int)AuthRadioButton.Value;
            switch (value)
            {
                case 1:
                    this.AuthLabel.Content = "Windows";
                    SetAuthMode(false);
                    break;
                default:
                    this.AuthLabel.Content = "SQL";
                    SetAuthMode(true);
                    break;
            }
        }

        private void SetAuthMode(bool value)
        {
            IsSqlAuth = value;
            if (value)
            {
                this.User.IsEnabled = true;
                this.Pass.IsEnabled = true;

                // Reset the content to 'sa' from blank
                this.User.Text = "sa";
            }
            else
            {
                // Disable the Username and Password textboxes as we don't need them...
                this.User.IsEnabled = false;
                this.Pass.IsEnabled = false;

                // Reset the content to blank
                this.User.Text = null;
                this.Pass.Password = null;
            }
        }
    }
}

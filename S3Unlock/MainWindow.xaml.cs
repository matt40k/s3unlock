/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.ComponentModel;
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
        private BackgroundWorker bw = new BackgroundWorker();
        private bool IsConn = false;

        private string _user;
        private string _pass;
        private string _server;
        private string _database;
        
        /// <summary>
        /// MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            sql = new SqlCmd();
            VersionLabel.Content = "Version: " + Version;
        }

        /// <summary>
        /// Button click action for connecting to SQL, or at leasting trying ;)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            _user = this.User.Text;
            _pass = this.Pass.Password;
            _server = this.Server.Text;
            _database = this.Database.Text;

            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;

            if (bw.IsBusy != true)
                bw.RunWorkerAsync();
        }

        /// <summary>
        /// Resets all the "locked" Agents or Deployments. The current tab is used to define if 
        /// it is a Agent or a Deployment.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetAllButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((string)(tc.SelectedItem as TabItem).Header)
            {
                case "Agents":
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
                    break;
                case "Deployments":
                    try
                    {
                        if (sql.UpdateAllS3DeploymentStatus)
                            GetLockAgents();
                        else
                            MessageBox.Show("Failed");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Failed");
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Resets only selected locks
        /// Again, users the selected tab to define if it's "Agent" or "Deployment" lock being reset.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Use the selected tab to defined if it's an 'Agent' reset or a 'Deployment'
            switch ((string) (tc.SelectedItem as TabItem).Header)
            {
                case "Agents":
                    int agentcnt = dataGridAgent.SelectedItems.Count;
                    for (int i = 0; i < agentcnt; i++)
                    {
                        DataRowView row = (DataRowView) dataGridAgent.SelectedItems[i];
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
                    }
                    break;
                case "Deployments":
                    int deploymentcnt = dataGridDeployment.SelectedItems.Count;
                    for (int i = 0; i < deploymentcnt; i++)
                    {
                        DataRowView row = (DataRowView) dataGridDeployment.SelectedItems[i];
                        try
                        {
                            if (sql.UpdateSingleS3DeploymentStatus(row["distributed_deployment_plan_guid"].ToString()))
                                GetLockDeployments();
                            else
                                MessageBox.Show("Failed");
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Failed");
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Gets the "locked" agents from the SOLUS3 database and unlocks the UI
        /// if they are any "locks".
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
        /// Gets the "locked" deployments from the SOLUS3 database and unlocks the UI
        /// if they are any "locks".
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
            var value = (int)AuthSlider.Value;
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

        /// <summary>
        /// Set the SQL authentication that the UI will use
        /// 
        ///   true  = SQL
        ///   false = Windows
        /// 
        /// </summary>
        /// <param name="value">true equals SQL auth, false equals Windows</param>
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

        /// <summary>
        /// Trys to connect to MS-SQL server as a backgroundworker task, this is so waiting for it to fail 
        /// doesn't lock the UI thread making the application appear not to respond.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;

            if (IsSqlAuth)
            {
                // Use SQL Authentication
                IsConn = sql.CreateConnection(_user, _pass, _server, _database);
            }
            else
            {
                // Use Windows Authentication
                IsConn = sql.CreateConnection(_server, _database);
            }
        }

        /// <summary>
        /// Once the backgroundwoker task is complete it checks if it is connected then locks the connection
        /// UI elements and queries the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Try again, user cancelled connection :(", "Cancelled onnecting...");
            }
            else if (!(e.Error == null))
            {
                MessageBox.Show("Try again, can't get connected :(", "Error connecting...");
            }
            else
            {
                if (IsConn)
                {
                    this.Connect.IsEnabled = false;
                    this.Database.IsEnabled = false;
                    this.Server.IsEnabled = false;
                    this.User.IsEnabled = false;
                    this.Pass.IsEnabled = false;
                    this.AuthSlider.IsEnabled = false;
                    this.tc.IsEnabled = true;
                    GetLockAgents();
                    GetLockDeployments();
                }
                else
                {
                    MessageBox.Show("Try again, can't get connected :(", "Error connecting...");
                }
            }
        }
    }
}

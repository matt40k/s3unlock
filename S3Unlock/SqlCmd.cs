/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Data;
using System.Data.SqlClient;

namespace Matt40k.S3Unlock
{
    internal class SqlCmd
    {
        private SqlConnection connection;

        /// <summary>
        /// Creates a connection to the Microsoft SQL Server database using SQL authentication 
        /// 
        /// </summary>
        /// <param name="user">SQL Username</param>
        /// <param name="password">SQL password</param>
        /// <param name="server">SQL Server name (eg. localhost\sims2012)</param>
        /// <param name="database">SQL database (eg. solus3_deployment_server)</param>
        /// <returns></returns>
        public bool CreateConnection(string user, string password, string server, string database)
        {
            try
            {
                connection = new SqlConnection("user id=" + user + ";" +
                                               "password=" + password + ";server=" + server + ";" +
                                               "database=" + database + ";Trusted_Connection=no;" +
                                               "connection timeout=300");

            }
            catch (Exception)
            {
                return false;
            }
            return OpenConnection;
        }

        /// <summary>
        /// Creates a MS-SQL connection to the Microsoft SQL Server database using Windows authentication 
        /// </summary>
        /// <param name="server">MS-SQL Server for example: localhost\sims2012</param>
        /// <param name="database">The S3 MS-SQL database name</param>
        /// <returns></returns>
        public bool CreateConnection(string server, string database)
        {
            try
            {
                connection = new SqlConnection("server=" + server + ";" +
                                               "database=" + database + ";Trusted_Connection=yes;" +
                                               "connection timeout=300");
            }
            catch (Exception)
            {
                return false;
            }
            return OpenConnection;
        }

        /// <summary>
        /// Checks is there is an open connection to the MS-SQL database and if not opens one
        /// </summary>
        private bool OpenConnection
        {
            get
            {
                try
                {
                    switch (connection.State)
                    {
                        case ConnectionState.Open:
                            return true;
                        case ConnectionState.Closed:
                            connection.Open();
                            return true;
                    }
                }
                catch (Exception)
                {

                }
                return false;
            }
        }

        /// <summary>
        /// Queryies the S3 database for status of agents with the status code of Install Queued (4) and Install Active (8)
        /// 
        ///  SOLUS3 Agent Status code
        ///  ========================
        ///    AgentOffline = 1
        ///    AgentOnline = 2
        ///    AgentInstallQueued = 4
        ///    AgentInstallActive = 8
        ///    AgentInstallFailed = 16
        ///    AgentNotInstalled = 32
        ///    AgentUninstallQueued = 64
        ///    AgentUninstallActive = 128
        ///    AgentUninstallFailed = 256
        ///    PendingRedirect = 512
        /// 
        /// </summary>
        public DataTable GetS3LockedAgents
        {
            get
            {
                DataTable dt = new DataTable();
                try
                {
                    SqlDataReader dataReader = null;
                    SqlCommand sqlCommand = new SqlCommand
                        (@"SELECT
                          [name]
                          ,[agent_guid]
                          ,[ipv4]
                          ,[ipv6]
                          ,[mac_address]
                          ,[last_update]
                          ,[last_communication]
                          ,[last_heartbeat]
                          ,[active]
                          ,convert(varchar(3), [major_version]) + '.' + convert(varchar(3), [minor_version]) + '.' + convert(varchar(3), [build_version]) as 'client_version'
                          ,[comment] as 'comment'
                          ,[message]
                        FROM
                          [solus3].[agent]
                        WHERE
                          [agent_status] in ( 4, 8 )", connection);
                    dataReader = sqlCommand.ExecuteReader();
                    dt.Load(dataReader);
                }
                catch (Exception)
                {
                    return null;
                }
                return dt;
            }
        }

        /// <summary>
        /// Updates all rows in the 'agent' table on the SOLUS3 database where the agent status is 
        /// set to install queued (4) or active (8) to install failed (16)
        /// </summary>
        /// <returns>true if successfully</returns>
        public bool UpdateAllS3AgentStatus
        {
            get
            {
                try
                {
                    SqlCommand sqlCommand =
                        new SqlCommand("update [solus3].[agent] set agent_status = 16 where agent_status in ( 4, 8 )",
                            connection);
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Updates a specific agent in the 'agent' table on the SOLUS3 database where the agent status is 
        /// install queued (4) or active (8) to install failed (16) where the GUID matches
        /// </summary>
        /// <param name="guid">S3 Agent GUID</param>
        /// <returns>true if successfully</returns>
        public bool UpdateSingleS3AgentStatus(string guid)
        {
            try
            {
                SqlCommand sqlCommand =
                    new SqlCommand(
                        "update [solus3].[agent] set agent_status = 16 where agent_status in ( 4, 8 ) and agent_guid = '" +
                        guid + "'", connection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Queryies the S3 database for status of deployments with the status code of InProgress (2) and RollBackInProgress (9)
        /// 
        /// SOLUS3 Deployment Status code
        /// =============================
        ///   Scheduled = 1
        ///   InProgress = 2
        ///   Cancelled = 3
        ///   Failed = 4
        ///   PartiallySuccessful = 5
        ///   Successful = 6
        ///   RolledBack = 7
        ///   NotDeployed = 8
        ///   RollBackInProgress = 9
        /// 
        /// </summary>
        public DataTable GetS3LockedDeployments
        {
            get
            {
                var dt = new DataTable();
                try
                {
                    SqlDataReader dataReader = null;
                    var sqlCommand = new SqlCommand
                        (@"SELECT
                          [distributed_deployment_plan_guid]                          
                          ,[description]
                          ,[created_on]
                          ,[scheduled_for]
                          ,[started_on]
                          ,[ended_on]
                          ,[deployable]
                          ,[current_stage]
                          ,[enable_backups]
                          ,[delete_backups]
                        FROM
                          [solus3].[distributed_deployment_plan]
                        WHERE
                          [status] in ( 2, 9 )", connection);
                    dataReader = sqlCommand.ExecuteReader();
                    dt.Load(dataReader);
                }
                catch (Exception)
                {

                    throw;
                }
                return dt;
            }
        }

        /// <summary>
        /// Updates all the rows in the 'deployments' table on the SOLUS3 database where the deployment  
        /// status is InProgress (2) or RollBackInProgress (9) to Failed (4)
        /// </summary>
        /// <returns>true if successfully</returns>
        public bool UpdateAllS3DeploymentStatus
        {
            get
            {
                try
                {
                    SqlCommand sqlCommand =
                        new SqlCommand(
                            "update [solus3].[distributed_deployment_plan] set status = 4 where agent_status in ( 2, 9 )",
                            connection);
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Updates a specific deployment in the 'deployments' table on the SOLUS3 database where the deployment
        /// status is InProgress (2) or RollBackInProgress (9) to Failed (4) where the 'distributed_deployment_plan_guid' matches
        /// </summary>
        /// <param name="guid">Deployment GUID</param>
        /// <returns>true if successfully</returns>
        public bool UpdateSingleS3DeploymentStatus(string guid)
        {
            try
            {
                SqlCommand sqlCommand =
                    new SqlCommand(
                        "update [solus3].[distributed_deployment_plan] set status = 4 where agent_status in ( 2, 9 ) and distributed_deployment_plan_guid = '" +
                        guid + "'", connection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Data;
using System.Data.SqlClient;

namespace S3Unlock
{
    class SqlCmd
    {
        private SqlConnection connection;

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

        private bool OpenConnection
        {
            get
            {
                try
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                    }
                    if (connection.State == ConnectionState.Open)
                    {
                        return true;
                    }
                    // put a message here.
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /*
         * AgentOffline = 1
         * AgentOnline = 2,
         * AgentInstallQueued = 4
         * AgentInstallActive = 8
         * AgentInstallFailed = 16
         * AgentNotInstalled = 32
         * AgentUninstallQueued = 64
         * AgentUninstallActive = 128
         * AgentUninstallFailed = 256
         * PendingRedirect = 512
         */
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
                          [agent_status] = 8", connection);
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

        public bool UpdateAll
        {
            get
            {
                try
                {
                    SqlCommand sqlCommand = new SqlCommand("update [solus3].[agent] set agent_status = 16 where agent_status = 8", connection);
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        public bool UpdateSingle(string guid)
        {
            try
            {
                SqlCommand sqlCommand = new SqlCommand("update [solus3].[agent] set agent_status = 16 where agent_status = 8 and agent_guid = '" + guid + "'", connection);
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
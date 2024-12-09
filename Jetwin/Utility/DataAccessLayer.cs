using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Jetwin.Database
{
    public class DataAccessLayer
    {
        private readonly string connectionString;
        public DataAccessLayer()
        {
            connectionString = ConfigurationManager.ConnectionStrings["SqlConnection"].ConnectionString;
        }
        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        //DML (INSERT, UPDATE, DELETE)
        //FOR INSERTING, UPDATING, OR DELETING A DATA INTO A SPECIFIC TABLE IN DATABASE
        public bool ExecuteNonQuery(string query, Dictionary<string, object> parameters)
        {
            try
            {
                using (var connection = GetConnection())
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    connection.Open();
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        //OVERLOADED METHOD FOR TRANSACTION
        public bool ExecuteNonQuery(string query, Dictionary<string, object> parameters, SqlTransaction transaction = null)
        {
            try
            {
                using (var connection = transaction == null ? GetConnection() : null) // Open connection only if no transaction
                {
                    if (connection != null) connection.Open();

                    using (var command = new SqlCommand(query, transaction?.Connection ?? connection))
                    {
                        if (transaction != null) command.Transaction = transaction;

                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value);
                            }
                        }
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        //DQL (SELECT)
        //FOR RETRIEVING DATA IN A SPECIFIC TABLE IN DATABASE
        public object ExecuteScalar(string query, Dictionary<string, object> parameters)
        {
            try
            {
                using (var connection = GetConnection())
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        //OVERLOADED METHOD FOR TRANSACTION
        public object ExecuteScalar(string query, Dictionary<string, object> parameters, SqlTransaction transaction = null)
        {
            try
            {
                using (var connection = transaction == null ? GetConnection() : null) // Open connection only if no transaction
                {
                    if (connection != null) connection.Open();

                    using (var command = new SqlCommand(query, transaction?.Connection ?? connection))
                    {
                        if (transaction != null) command.Transaction = transaction;

                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value);
                            }
                        }
                        return command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        //DQL (SELECT)
        //FOR RETRIEVING ALL DATA OF A SPECIFIC TABLE IN DATABASE AND PUTTING IT IN INTERFACE TABLE FOR VIEWING
        public DataTable ExecuteQuery(string query, Dictionary<string, object> parameters)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                using (var adapter = new SqlDataAdapter(command))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }      
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        //OVERLOADED METHOD FOR TRANSACTION
        public DataTable ExecuteQuery(string query, Dictionary<string, object> parameters, SqlTransaction transaction = null)
        {
            try
            {
                using (var connection = transaction == null ? GetConnection() : null) // Open connection only if no transaction
                {
                    if (connection != null) connection.Open();

                    using (var command = new SqlCommand(query, transaction?.Connection ?? connection))
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        if (transaction != null) command.Transaction = transaction;

                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value);
                            }
                        }
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        //FOR TRANSACTION
        public void ExecuteTransaction(Action<SqlTransaction> transactionalWork)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        transactionalWork(transaction);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

    }
}

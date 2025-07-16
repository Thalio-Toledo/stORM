using System.Data;
using System.Data.SqlClient;
using Dapper;


namespace BonesCore.ConfigOptions
{
    public class DBConnectionOptions
    {
        public string? DefaultConnection { get; set; }

        private SqlConnection _connection;
        private SqlTransaction _transaction;
        private bool _isCommitted = false;

        public string GetConnection() => DefaultConnection;

        public string Query(string script)
        {
            try
            {
                using (var connection = new SqlConnection(GetConnection()))
                {
                    connection.Open();
                    string result = connection.QueryFirst<string>(script);

                    connection.Close();

                    if (result is null) return "";

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<string> QueryAsync(string script)
        {
            try
            {
                using (var connection = new SqlConnection(GetConnection()))
                {
                    connection.Open();
                    string result = await connection.QueryFirstAsync<string>(script);

                    connection.Close();

                    if(result is null) return "";

                    return result;
                    
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public IEnumerable<T> Query<T>(string script, object parameters)
        {
            if (_connection is null || _connection.State == ConnectionState.Closed)
            {
                _connection = new SqlConnection(GetConnection());
                _connection.Open();
            }

            try
            {
                var result = _connection.Query<T>(script, parameters);

                CloseConnection();

                return result;
            }
            catch (Exception ex)
            {
                CloseConnection();

                throw new Exception(ex.Message);
            }
        }

        public string QueryFirst(string script, object parameters)
        {
            if (_connection is null || _connection.State == ConnectionState.Closed)
            {
                _connection = new SqlConnection(GetConnection());
                _connection.Open();
            }

            try
            {
                string result = _connection.QueryFirst<string>(script, parameters);

                CloseConnection();

                if (result is null) return "";

                return result;
            }
            catch (Exception ex)
            {
                CloseConnection();

                throw new Exception(ex.Message);
            }
        }

        public string Query(string script, object parameters = null)
        {
            if (_connection is null || _connection.State == ConnectionState.Closed)
            {
                _connection = new SqlConnection(GetConnection());
                _connection.Open();
            }

            try
            {
                string result =  _connection.QueryFirst<string>(script, parameters);

                CloseConnection();

                if (result is null) return "";

                return result;
            }
            catch (Exception ex)
            {
                CloseConnection();

                throw new Exception(ex.Message);
            }
        }

        public int Insert(string script)
        {
            if (_connection is null || _connection.State == ConnectionState.Closed)
            {
                _connection = new SqlConnection(GetConnection());
                _connection.Open();
            }

            if (_transaction is null || _isCommitted is true)
            {
                _transaction = _connection.BeginTransaction();
                _isCommitted = false;
            }
            try
            {
                var result = _connection.QueryFirst<int>(script, transaction : _transaction);
                //Commit();
                //CloseConnection();
                return result;
            }
            catch(Exception ex)
            {
                Rollback();

                throw new Exception(ex.Message);
            }
        }

        public int Execute(string script)
        {
            if (_connection is null || _connection.State == ConnectionState.Closed)
            {
                _connection = new SqlConnection(GetConnection());
                _connection.Open();
            }

            if (_transaction is null || _isCommitted is true)
            {
                _transaction = _connection.BeginTransaction();
                _isCommitted = false;
            }

            try
            {
                var result = _connection.Execute(script, transaction: _transaction);

                if (result > 1)
                {
                    Rollback();
                    throw new Exception("More one row affected");
                }

                //Commit();
                //CloseConnection();
                return result;

            }
            catch (Exception ex)
            {
                Rollback();

                throw new Exception(ex.Message);
            }

        }

        public void Commit()
        {
            _transaction.Commit();
            _isCommitted = true;
            CloseConnection();
        }

        public void Rollback()
        {
            _transaction.Rollback();
            _isCommitted = true;
            CloseConnection();
        }


        private void CloseConnection()
        {
            _connection.Close(); 
            _connection.Dispose();
        }


    }
}

using System;
using System.Data.SqlServerCe;

namespace AccelLibTest.Stubs
{
    /// <summary>
    /// Тестовая база данных
    /// </summary>
    public class TestDatabaseStub
    {
        private readonly string _testDb;

        public TestDatabaseStub(string testDbFileName)
        {
            _testDb = testDbFileName;
        }

        #region DB helper functions

        /// <summary>
        /// Проверяет существует ли файл базы данных
        /// </summary>
        /// <param name="dbFileName"></param>
        /// <returns></returns>
        public bool IsDatabaseExists()
        {
            return System.IO.File.Exists(_testDb);
        }

        /// <summary>
        /// Проверяет что существует таблица в базе данных
        /// Поскольку SqlCe не поддерживает If Exists
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool IsTableExist(string tableName)
        {
            var connStr = String.Format("Data Source = '{0}';", _testDb);

            using (var conn = new SqlCeConnection(connStr))
            {
                conn.Open();
                var command = new SqlCeCommand();
                command.Connection = conn;
                command.CommandText = @"SELECT COUNT(TABLE_NAME) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
                command.Parameters.AddWithValue("@tableName", tableName);
                var reader = command.ExecuteReader();
                reader.Read();
                return reader.GetInt32(0) > 0;
            }
        }

        /// <summary>
        /// Считает кол-во элементов в таблице
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public int GetIdCount(string tableName)
        {
            var connStr = String.Format("Data Source = '{0}';", _testDb);

            using (var conn = new SqlCeConnection(connStr))
            {
                conn.Open();
                var command = new SqlCeCommand();
                command.Connection = conn;
                command.CommandText = String.Format("SELECT COUNT(id) FROM {0}", tableName);
                var reader = command.ExecuteReader();
                reader.Read();
                return reader.GetInt32(0);
            }
        }

        #endregion

        #region InitTest DB

        /// <summary>
        /// Создание файла БД
        /// </summary>
        public void CreateDatabase()
        {
            var connStr = String.Format("Data Source = '{0}';", _testDb);
            var engine = new SqlCeEngine(connStr);
            engine.CreateDatabase();
        }

        public void InitTestSchema()
        {
            var connStr = String.Format("Data Source = '{0}';", _testDb);
            using (var conn = new SqlCeConnection(connStr))
            {
                conn.Open();
                var command = new SqlCeCommand();
                command.Connection = conn;
                command.CommandText =
                    @"CREATE TABLE accel_data (
                                     id INT IDENTITY NOT NULL PRIMARY KEY,
                                     date DATETIME, 
                                     Ax Float,Ay Float
                                     )";
                command.ExecuteNonQuery();

                command.CommandText = @"CREATE TABLE accel_params (
                                         id INT IDENTITY NOT NULL PRIMARY KEY,
                                         date DATETIME, 
                                         sensorNumber smallint,
                                         offsetX Float,offsetY Float,
                                         gravityX Float,gravityY Float
                                     )";
                command.ExecuteNonQuery();

                command.CommandText = @"CREATE TABLE calibr_result (
                                         id INT IDENTITY NOT NULL PRIMARY KEY,
                                         accelDataId INT,
                                         accelParamsId INT
                                     )";
                command.ExecuteNonQuery();
            }
        }

        #endregion

        #region Populate Test DB

        /// <summary>
        /// Заполняет БД тестовыми данными акселерометра
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public void PopulateTestAccelData()
        {
            var connStr = String.Format("Data Source = '{0}';", _testDb);

            using (var conn = new SqlCeConnection(connStr))
            {
                conn.Open();
                var command = new SqlCeCommand();
                command.Connection = conn;
                command.CommandText = @"INSERT INTO accel_data (date, Ax,Ay) VALUES(@date,@Ax,@Ay)";
                command.Parameters.AddWithValue("@date", new DateTime(2012, 1, 1) - TimeSpan.FromDays(10));
                command.Parameters.AddWithValue("@Ax", 22.0);
                command.Parameters.AddWithValue("@Ay", 50.0);
                command.ExecuteNonQuery();

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@date", new DateTime(2012, 1, 1) + TimeSpan.FromDays(10));
                command.Parameters.AddWithValue("@Ax", 22.0);
                command.Parameters.AddWithValue("@Ay", 45.0);
                command.ExecuteNonQuery();

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@date", new DateTime(2012, 1, 1) - TimeSpan.FromDays(1));
                command.Parameters.AddWithValue("@Ax", -10.0);
                command.Parameters.AddWithValue("@Ay", -50.0);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Заполняет БД тестовыми калибровочными данными
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public void PopulateTestAccelParams()
        {
            var connStr = String.Format("Data Source = '{0}';", _testDb);

            using (var conn = new SqlCeConnection(connStr))
            {
                conn.Open();
                var command = new SqlCeCommand();
                command.Connection = conn;
                command.CommandText = @"INSERT INTO accel_params (date,sensorNumber, offsetX,offsetY,gravityX,gravityY) VALUES(@date,@sensorNumber,@offsetX,@offsetY,@gravityX,@gravityY)";

                command.Parameters.AddWithValue("@date", new DateTime(2012, 1, 1) - TimeSpan.FromDays(10));
                command.Parameters.AddWithValue("@sensorNumber", 1);
                command.Parameters.AddWithValue("@offsetX", 10.0);
                command.Parameters.AddWithValue("@offsetY", 10.0);
                command.Parameters.AddWithValue("@gravityX", 500.0);
                command.Parameters.AddWithValue("@gravityY", 500.0);
                command.ExecuteNonQuery();

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@date", new DateTime(2012, 1, 1) + TimeSpan.FromDays(10));
                command.Parameters.AddWithValue("@sensorNumber", 1);
                command.Parameters.AddWithValue("@offsetX", 10.0);
                command.Parameters.AddWithValue("@offsetY", 10.0);
                command.Parameters.AddWithValue("@gravityX", 500.0);
                command.Parameters.AddWithValue("@gravityY", 500.0);
                command.ExecuteNonQuery();

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@date", new DateTime(2012, 1, 1) - TimeSpan.FromDays(1));
                command.Parameters.AddWithValue("@sensorNumber", 2);
                command.Parameters.AddWithValue("@offsetX", 10.0);
                command.Parameters.AddWithValue("@offsetY", 10.0);
                command.Parameters.AddWithValue("@gravityX", 500.0);
                command.Parameters.AddWithValue("@gravityY", 500.0);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Заполняет БД тестовыми результатами калибровки
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public void PopulateTestCalibrationResult()
        {
            var connStr = String.Format("Data Source = '{0}';", _testDb);

            using (var conn = new SqlCeConnection(connStr))
            {
                conn.Open();
                var command = new SqlCeCommand();
                command.Connection = conn;
                command.CommandText = @"INSERT INTO calibr_result (accelDataId,accelParamsId) VALUES(@accelDataId,@accelParamsId)";

                command.Parameters.AddWithValue("@accelDataId", 1);
                command.Parameters.AddWithValue("@accelParamsId", 1);
                command.ExecuteNonQuery();

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@accelDataId", 2);
                command.Parameters.AddWithValue("@accelParamsId", 1);
                command.ExecuteNonQuery();

            }
        }
        #endregion

        public void DeleteDatabase()
        {
            if (System.IO.File.Exists(_testDb))
            {
                System.IO.File.Delete(_testDb);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;
using AccelLib.Models;

namespace AccelLib
{
    /// <summary>
    /// Инкапсулирует сохранение и выборку данных из SqlCe базы данных
    /// </summary>
    public class LocalDatabaseStorage : IDisposable
    {

        #region Private fields

        private readonly ILogger _logger;
        private readonly string _connectionString;
        private readonly string _databaseFile;
        private SqlCeConnection _connection;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор.
        /// Если база данных не существует создает ее.
        /// </summary>
        /// <param name="databaseFile">путь к файлу с SqlCe базой данных</param>
        /// <param name="logger"></param>
        public LocalDatabaseStorage(string databaseFile, ILogger logger)
        {
            _logger = logger;
            _connectionString = String.Format("Data Source = '{0}';", databaseFile);
            _databaseFile = databaseFile;
            _connection = new SqlCeConnection(_connectionString);
        }

        #endregion

        public void Open()
        {
            // создаем файл БД если несуещствует
            if (!File.Exists(_databaseFile))
            {
                CreateDatabase();
            }
            _connection.Open();
            // создаем схему БД если надо
            InitSchema();
        }

        public void Close()
        {
            _connection.Close();
        }

        #region Инциализация базы данных

        /// <summary>
        /// Проверяет что существует таблица в базе данных
        /// Поскольку SqlCe не поддерживает If Exists
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private bool IsTableExist(string tableName)
        {
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"SELECT COUNT(TABLE_NAME) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
            command.Parameters.AddWithValue("@tableName", tableName);
            var reader = command.ExecuteReader();
            reader.Read();
            return reader.GetInt32(0) > 0;
        }

        /// <summary>
        /// Создает таблицу данных акселерометра
        /// </summary>
        private void CreateTableAccelData()
        {
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"CREATE TABLE accel_data (
                                         id INT IDENTITY NOT NULL PRIMARY KEY,
                                         date DATETIME, 
                                         Ax Float,Ay Float
                                     )";
            command.ExecuteNonQuery();
            _logger.Log("Создана таблица accel_data");
        }

        /// <summary>
        /// Создает таблицу калибровочных данных
        /// </summary>
        private void CreateTableAccelParams()
        {
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"CREATE TABLE accel_params (
                                         id INT IDENTITY NOT NULL PRIMARY KEY,
                                         date DATETIME, 
                                         sensorNumber smallint,
                                         offsetX Float,offsetY Float,
                                         gravityX Float,gravityY Float
                                     )";
            command.ExecuteNonQuery();
            _logger.Log("Создана таблица accel_params");

        }

        /// <summary>
        /// Создает вспомогательную таблицу для связывания калибровочных данных и данных датчика
        /// </summary>
        private void CreateTableCalibrationResult()
        {
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"CREATE TABLE calibr_result (
                                         id INT IDENTITY NOT NULL PRIMARY KEY,
                                         accelDataId INT,
                                         accelParamsId INT
                                     )";
            command.ExecuteNonQuery();
            _logger.Log("Создана таблица calibr_result");
        }

        /// <summary>
        /// Создание физического файла базы данных
        /// </summary>
        private void CreateDatabase()
        {
            var engine = new SqlCeEngine(_connectionString);
            engine.CreateDatabase();
            _logger.Log(String.Format("Создана база данных: {0}", _connectionString));
        }

        /// <summary>
        /// Инициализирует структуру БД
        /// </summary>
        private void InitSchema()
        {
            if (!IsTableExist("accel_data"))
            {
                CreateTableAccelData();
            }
            if (!IsTableExist("accel_params"))
            {
                CreateTableAccelParams();
            }
            if (!IsTableExist("calibr_result"))
            {
                CreateTableCalibrationResult();
            }
        }

        #endregion

        #region Запись AccelData

        /// <summary>
        /// Сохраняет в базе данные акселерометра
        /// </summary>
        /// <param name="data"></param>
        public void SaveAccelData(AccelData data)
        {
            if (data.Id > 0)
            {
                UpdateAccelData(data);
            }
            else
            {
                CreateAccelData(data);
            }
        }

        /// <summary>
        /// Создает в базе запись
        /// </summary>
        /// <param name="data"></param>
        private void CreateAccelData(AccelData data)
        {
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"INSERT INTO accel_data (date, Ax,Ay) VALUES(@date,@Ax,@Ay)";
            command.Parameters.AddWithValue("@date", data.Date);
            command.Parameters.AddWithValue("@Ax", data.Ax);
            command.Parameters.AddWithValue("@Ay", data.Ay);
            command.ExecuteNonQuery();
            command.CommandText = "SELECT @@IDENTITY";
            var id = command.ExecuteScalar();
            if (id != null)
            {
                data.Id = Convert.ToInt32((decimal)id); // записываем идентификатор записи в базе
            }

        }

        /// <summary>
        /// Обновляет запись в базе
        /// </summary>
        /// <param name="data"></param>
        private void UpdateAccelData(AccelData data)
        {
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"UPDATE accel_data SET date=@date,Ax=@Ax,Ay=@Ay WHERE id = @id";
            command.Parameters.AddWithValue("@id", data.Id);
            command.Parameters.AddWithValue("@date", data.Date);
            command.Parameters.AddWithValue("@Ax", data.Ax);
            command.Parameters.AddWithValue("@Ay", data.Ay);
            command.ExecuteNonQuery();

        }

        #endregion

        #region Выборка AccelData

        /// <summary>
        /// Возвращает список всех записанных данных акселерометра
        /// </summary>
        /// <returns></returns>
        public IList<AccelData> GetAccelData()
        {
            var result = new List<AccelData>();
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"SELECT date,Ax,Ay FROM accel_data";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var data = new AccelData();
                data.Date = reader.GetDateTime(0);
                data.Ax = reader.GetDouble(1);
                data.Ay = reader.GetDouble(2);
                result.Add(data);
            }

            return result;
        }

        /// <summary>
        /// Возвращает список данных акселерометра записанных в промежуток времени (границы промежутка включительно)
        /// </summary>
        /// <param name="from">начало промежутка</param>
        /// <param name="till">конец промежутка</param>
        /// <returns></returns>
        public IList<AccelData> GetAccelDataByDateSpan(DateTime from, DateTime till)
        {
            var result = new List<AccelData>();
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"SELECT date,Ax,Ay FROM accel_data WHERE date >= @from AND date <= @till";
            command.Parameters.AddWithValue("@from", from);
            command.Parameters.AddWithValue("@till", till);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var data = new AccelData();
                data.Date = reader.GetDateTime(0);
                data.Ax = reader.GetDouble(1);
                data.Ay = reader.GetDouble(2);
                result.Add(data);
            }

            return result;
        }

        #endregion

        #region Запись AccelParams

        /// <summary>
        /// Сохраняет в базе калибровочные данные
        /// </summary>
        /// <param name="data"></param>
        public void SaveAccelParams(AccelParams data)
        {
            if (data.Id > 0)
            {
                UpdateAccelParams(data);
            }
            else
            {
                CreateAccelParams(data);
            }
        }

        /// <summary>
        /// Создает в базе запись
        /// </summary>
        /// <param name="data"></param>
        private void CreateAccelParams(AccelParams data)
        {
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"INSERT INTO accel_params (date,sensorNumber, offsetX,offsetY,gravityX,gravityY) VALUES(@date,@sensorNumber,@offsetX,@offsetY,@gravityX,@gravityY)";
            command.Parameters.AddWithValue("@date", data.Date);
            command.Parameters.AddWithValue("@sensorNumber", data.SensorNumber);
            command.Parameters.AddWithValue("@offsetX", data.OffsetX);
            command.Parameters.AddWithValue("@offsetY", data.OffsetY);
            command.Parameters.AddWithValue("@gravityX", data.GravityX);
            command.Parameters.AddWithValue("@gravityY", data.GravityY);
            command.ExecuteNonQuery();
            command.CommandText = "SELECT @@IDENTITY";
            var id = command.ExecuteScalar();
            if (id != null)
            {
                data.Id = Convert.ToInt32((decimal)id); // записываем идентификатор записи в базе
            }

        }

        /// <summary>
        /// Обновляет запись в базе
        /// </summary>
        /// <param name="data"></param>
        private void UpdateAccelParams(AccelParams data)
        {
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"UPDATE accel_params SET date=@date,sensorNumber=@sensorNumber, offsetX=@offsetX,offsetY=@offsetY,gravityX=@gravityX,gravityY=@gravityY WHERE id = @id";
            command.Parameters.AddWithValue("@id", data.Id);
            command.Parameters.AddWithValue("@date", data.Date);
            command.Parameters.AddWithValue("@sensorNumber", data.SensorNumber);
            command.Parameters.AddWithValue("@offsetX", data.OffsetX);
            command.Parameters.AddWithValue("@offsetY", data.OffsetY);
            command.Parameters.AddWithValue("@gravityX", data.GravityX);
            command.Parameters.AddWithValue("@gravityY", data.GravityY);
            command.ExecuteNonQuery();

        }

        #endregion

        #region Выборка AccelParams

        /// <summary>
        /// Возвращает список всех записанных калибровочных данных
        /// </summary>
        /// <returns></returns>
        public IList<AccelParams> GetAccelParams()
        {
            var result = new List<AccelParams>();
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"SELECT date,sensorNumber,offsetX,offsetY,gravityX,gravityY FROM accel_params";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var data = new AccelParams();
                data.Date = reader.GetDateTime(0);
                data.SensorNumber = (UInt16)reader.GetInt16(1);
                data.OffsetX = reader.GetDouble(2);
                data.OffsetY = reader.GetDouble(3);
                data.GravityX = reader.GetDouble(4);
                data.GravityY = reader.GetDouble(5);
                result.Add(data);
            }
            return result;
        }

        /// <summary>
        /// Возвращает калибровочных данных для указанного номера датчика
        /// </summary>
        /// <param name="sensorNumber">номер датчика</param>
        /// <returns></returns>
        public IList<AccelParams> GetAccelParamsBySensorNumber(UInt16 sensorNumber)
        {
            var result = new List<AccelParams>();
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"SELECT date,sensorNumber,offsetX,offsetY,gravityX,gravityY FROM accel_params WHERE sensorNumber = @sensorNumber";
            command.Parameters.AddWithValue("@sensorNumber", sensorNumber);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var data = new AccelParams();
                data.Date = reader.GetDateTime(0);
                data.SensorNumber = (UInt16)reader.GetInt16(1);
                data.OffsetX = reader.GetDouble(2);
                data.OffsetY = reader.GetDouble(3);
                data.GravityX = reader.GetDouble(4);
                data.GravityY = reader.GetDouble(5);
                result.Add(data);
            }

            return result;
        }

        /// <summary>
        /// Возвращает список калибровочных данных записанных в промежуток времени (границы промежутка включительно)
        /// </summary>
        /// <param name="from">начало промежутка</param>
        /// <param name="till">конец промежутка</param>
        /// <returns></returns>
        public IList<AccelParams> GetAccelParamsByDateSpan(DateTime from, DateTime till)
        {
            var result = new List<AccelParams>();
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"SELECT date,sensorNumber,offsetX,offsetY,gravityX,gravityY FROM accel_params WHERE date >= @from AND date <= @till";
            command.Parameters.AddWithValue("@from", from);
            command.Parameters.AddWithValue("@till", till);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var data = new AccelParams();
                data.Date = reader.GetDateTime(0);
                data.SensorNumber = (UInt16)reader.GetInt16(1);
                data.OffsetX = reader.GetDouble(2);
                data.OffsetY = reader.GetDouble(3);
                data.GravityX = reader.GetDouble(4);
                data.GravityY = reader.GetDouble(5);
                result.Add(data);
            }

            return result;
        }

        #endregion

        #region Запись результатов калибровки

        /// <summary>
        /// Сохраняет данные при калибровке датчика.
        /// </summary>
        /// <remarks>Связывает и сохраняет данные для последующей выборки  GetCalibrationResultByParamsId</remarks>
        /// <param name="parameters">результат калбировки</param>
        /// <param name="clusters">данные используемые для калибровки</param>
        public void SaveCalibrationResult(CalibrationResult data)
        {
            // сохраняем/обновляем параметры
            SaveAccelParams(data.Parameters);
            // сохраняем/обновляем кластеры
            foreach (var accelData in data.Clusters)
            {
                SaveAccelData(accelData);
            }
            // удаляем старые связи между кластерами и калибровочными данными
            DeleteCalibrationResult(data.Parameters);
            // создаем записи во вспомогательной таблице
            CreateCalibrationResult(data);
        }

        /// <summary>
        /// Удаляет результатов калбировки для заданных параметров
        /// </summary>
        /// <param name="parameters"></param>
        private void DeleteCalibrationResult(AccelParams parameters)
        {
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"DELETE FROM calibr_result WHERE accelParamsId = @id";
            command.Parameters.AddWithValue("@id", parameters.Id);
            command.ExecuteNonQuery();

        }

        /// <summary>
        /// Создает записи в вспомогательной таблицы для связи калибровочных параметров и кластеров
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="clusters"></param>
        private void CreateCalibrationResult(CalibrationResult data)
        {
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"INSERT INTO calibr_result (accelDataId,accelParamsId) VALUES(@accelDataId,@accelParamsId)";

            foreach (var accelData in data.Clusters)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@accelDataId", accelData.Id);
                command.Parameters.AddWithValue("@accelParamsId", data.Parameters.Id);
                command.ExecuteNonQuery();
            }

        }
        #endregion

        #region Выборка результатов калибровки

        /// <summary>
        /// Делает выборку по результатам калибровки для указанного идентификатора калибровочных параметров
        /// </summary>
        public CalibrationResult GetCalibrationResultByParams(AccelParams parameters)
        {
            var result = new CalibrationResult {Parameters = parameters};
            var command = new SqlCeCommand();
            command.Connection = _connection;
            command.CommandText = @"SELECT data.Id, data.date, data.Ax, data.Ay
                                    FROM calibr_result result JOIN accel_data data
                                        ON (result.accelDataId = data.id) 
                                    WHERE result.accelParamsId = @paramsId";
            command.Parameters.AddWithValue("@paramsId", parameters.Id);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var data = new AccelData();
                data.Id = reader.GetInt32(0);
                data.Date = reader.GetDateTime(1);
                data.Ax = reader.GetDouble(2);
                data.Ay = reader.GetDouble(3);
                result.Clusters.Add(data);
            }
            return result;
        }

        #endregion

        #region Dispose

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);

        }

        protected virtual void Dispose(bool disposing)
        {

            if (!_disposed)
            {
                if (disposing)
                {
                    if (_connection != null)
                        _connection.Close();
                }

                _connection = null;
                _disposed = true;
            }
        }

        #endregion
    }
}

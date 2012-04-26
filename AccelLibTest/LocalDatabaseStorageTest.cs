using System.Collections.Generic;
using System.Data.SqlServerCe;
using AccelLib;
using AccelLib.Models;
using AccelLibTest.Stubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace AccelLibTest
{
    
    [TestClass()]
    public class LocalDatabaseStorageTest
    {

        private const string _testDbFile = "test.sdf";
        private TestDatabaseStub _testDb;

        #region SetUp/TearDown

        [TestInitialize]
        public void SetUp()
        {
            _testDb = new TestDatabaseStub(_testDbFile);
        }

        [TestCleanup]
        public void TearDown()
        {
            _testDb.DeleteDatabase();
        }

        #endregion

        #region ConstructorTest

        /// <summary>
        ///Тест конструктора
        ///</summary>
        [TestMethod]
        public void ConstructorWidthDbCreationTest()
        {
            string databaseFile = _testDbFile;
            ILogger logger = new LoggerStub(); 
            LocalDatabaseStorage target = new LocalDatabaseStorage(databaseFile, logger);
            
        }

       
        #endregion

        #region Open Test

        /// <summary>
        /// Тест открытия БД с созданием базы данных и схемы
        /// </summary>
        [TestMethod]
        public void OpenTestWithFullDbCreation()
        {
            string databaseFile = _testDbFile;
            ILogger logger = new LoggerStub();

            LocalDatabaseStorage target = new LocalDatabaseStorage(databaseFile, logger);

            target.Open();

            Assert.IsTrue(_testDb.IsDatabaseExists(), "проверка создания файла базы данных");
            Assert.IsTrue(_testDb.IsTableExist("accel_data"), "проверка создания таблицы данных акселерометров");
            Assert.IsTrue(_testDb.IsTableExist("accel_params"), "проверка создания таблицы калибровочных данных");
            Assert.IsTrue(_testDb.IsTableExist("calibr_result"), "проверка создания таблица для результатов калибровки");

            target.Close();
        }

        /// <summary>
        /// Тест открытия БД с схемы в существующей БД
        /// </summary>
        [TestMethod]
        public void OpenTestWithSchemaCreation()
        {
            _testDb.CreateDatabase();
            string databaseFile = _testDbFile;
            ILogger logger = new LoggerStub();

            LocalDatabaseStorage target = new LocalDatabaseStorage(databaseFile, logger);

            target.Open();

            Assert.IsTrue(_testDb.IsDatabaseExists(), "проверка создания файла базы данных");
            Assert.IsTrue(_testDb.IsTableExist("accel_data"), "проверка создания таблицы данных акселерометров");
            Assert.IsTrue(_testDb.IsTableExist("accel_params"), "проверка создания таблицы калибровочных данных");
            Assert.IsTrue(_testDb.IsTableExist("calibr_result"), "проверка создания таблица для результатов калибровки");

            target.Close();
        }

        #endregion

        #region Save AccelData Tests

        /// <summary>
        ///Тест сохранения данных акселерометра в БД.
        /// 
        ///</summary>
        [TestMethod]
        public void SaveAccelDataTest()
        {
            string databaseFile = _testDbFile;
            ILogger logger = new LoggerStub();
            var target = new LocalDatabaseStorage(databaseFile, logger);
            target.Open();
            var data = new AccelData(10.0, 50.0);
            data.Date = DateTime.Now;

            target.SaveAccelData(data);


            var idCount = _testDb.GetIdCount("accel_data");
            Assert.AreEqual(idCount,1,"проверяем, что запись добавлена");
            Assert.AreEqual(1, data.Id, "проверяем, что изменен идентификатор записи в базе");

            target.Close();
        }

        /// <summary>
        /// Тест сохранения данных акселерометра в БД, несколько раз.
        /// Проверяется, что в первый раз создается запись в базе, потом она обновялется.
        /// 
        ///</summary>
        [TestMethod]
        public void MultipleSaveAccelDataTest()
        {
            string databaseFile = _testDbFile;
            ILogger logger = new LoggerStub();
            var target = new LocalDatabaseStorage(databaseFile, logger);
            target.Open();
            var data = new AccelData(10.0, 50.0);
            data.Date = DateTime.Now;

            target.SaveAccelData(data);
            target.SaveAccelData(data);
            target.SaveAccelData(data);

            var idCount = _testDb.GetIdCount("accel_data");
            Assert.AreEqual(idCount, 1, "проверяем, что запись не была добавлена несколько раз");
            Assert.AreEqual(1, data.Id, "проверяем, что изменен идентификатор записи в базе не изменился");

            target.Close();
        }

        #endregion

        #region Get AccelData Tests

        // <summary>
        /// Тест выборки всех записей данных акселерометра из БД
        ///</summary>
        [TestMethod]
        public void GetAccelDataTest()
        {
            _testDb.CreateDatabase();
            _testDb.InitTestSchema();
            _testDb.PopulateTestAccelData();
            string databaseFile = _testDbFile;
            ILogger logger = new LoggerStub();
            var target = new LocalDatabaseStorage(databaseFile, logger);
            target.Open();

            var actual = target.GetAccelData();

            Assert.AreEqual(3,actual.Count);

            target.Close();
        }

        /// <summary>
        /// Тест выборки данных акселерометра из БД по времени записи
        ///</summary>
        [TestMethod]
        public void GetAccelDataByDateSpanTest()
        {
            _testDb.CreateDatabase();
            _testDb.InitTestSchema();
            _testDb.PopulateTestAccelData();
            string databaseFile = _testDbFile;
            ILogger logger = new LoggerStub();
            var target = new LocalDatabaseStorage(databaseFile, logger);
            target.Open();

            var actual = target.GetAccelDataByDateSpan(new DateTime(2012, 1, 1) - TimeSpan.FromDays(10), new DateTime(2012, 1, 1));

            Assert.AreEqual(2, actual.Count);

            target.Close();
        }

        #endregion

        #region Save AccelParams Tests

        /// <summary>
        ///Тест сохранения калибровочных данных в БД.
        /// 
        ///</summary>
        [TestMethod]
        public void SaveAccelParamsTest()
        {
            string databaseFile = _testDbFile;
            ILogger logger = new LoggerStub();
            var target = new LocalDatabaseStorage(databaseFile, logger);
            target.Open();
            var data = new AccelParams { Date = DateTime.Now };

            target.SaveAccelParams(data);

            var idCount = _testDb.GetIdCount("accel_params");
            Assert.AreEqual(idCount, 1, "проверяем, что запись добавлена");
            Assert.AreEqual(1, data.Id, "проверяем, что изменен идентификатор записи в базе");

            target.Close();
        }

        /// <summary>
        /// Тест сохранения калибровочных данных в БД, несколько раз.
        /// Проверяется, что в первый раз создается запись в базе, потом она обновялется.
        /// 
        ///</summary>
        [TestMethod]
        public void MultipleSaveAcceParamsTest()
        {
            string databaseFile = _testDbFile;
            ILogger logger = new LoggerStub();
            var target = new LocalDatabaseStorage(databaseFile, logger);
            target.Open();
            var data = new AccelParams { Date = DateTime.Now };

            target.SaveAccelParams(data);
            target.SaveAccelParams(data);
            target.SaveAccelParams(data);

            var idCount = _testDb.GetIdCount("accel_params");
            Assert.AreEqual(idCount, 1, "проверяем, что запись не была добавлена несколько раз");
            Assert.AreEqual(1, data.Id, "проверяем, что изменен идентификатор записи в базе не изменился");

            target.Close();
        }

        #endregion

        #region Get AccelParams Tests

        // <summary>
        /// Тест выборки всех записей калибровочных данных из БД
        ///</summary>
        [TestMethod]
        public void GetAccelParamsTest()
        {
            _testDb.CreateDatabase();
            _testDb.InitTestSchema();
            _testDb.PopulateTestAccelParams();
            string databaseFile = _testDbFile;
            ILogger logger = new LoggerStub();
            var target = new LocalDatabaseStorage(databaseFile, logger);
            target.Open();

            var actual = target.GetAccelParams();

            Assert.AreEqual(3, actual.Count);

            target.Close();
        }

        /// <summary>
        /// Тест выборки калибровочных данных из БД по времени записи
        ///</summary>
        [TestMethod]
        public void GetAccelParamsByDateSpanTest()
        {
            _testDb.CreateDatabase();
            _testDb.InitTestSchema();
            _testDb.PopulateTestAccelParams();
            string databaseFile = _testDbFile;
            ILogger logger = new LoggerStub();
            var target = new LocalDatabaseStorage(databaseFile, logger);
            target.Open();
            var data = new AccelData(10.0, 50.0);
            data.Date = DateTime.Now;

            var actual = target.GetAccelParamsByDateSpan(new DateTime(2012, 1, 1) - TimeSpan.FromDays(10), new DateTime(2012, 1, 1));

            Assert.AreEqual(2, actual.Count);

            target.Close();
        }

        /// <summary>
        /// Тест выборки калибровочных данных из БД по номеру датчика
        ///</summary>
        [TestMethod]
        public void GetAccelParamsBySensorNumberTest()
        {
            _testDb.CreateDatabase();
            _testDb.InitTestSchema();
            _testDb.PopulateTestAccelParams();
            string databaseFile = _testDbFile;
            ILogger logger = new LoggerStub();
            var target = new LocalDatabaseStorage(databaseFile, logger);
            target.Open();
            var data = new AccelData(10.0, 50.0);
            data.Date = DateTime.Now;

            var actual = target.GetAccelParamsBySensorNumber(1);

            Assert.AreEqual(2, actual.Count);

            target.Close();
        }

        #endregion

        #region SaveCalibrationResult Test

        /// <summary>
        /// Тест сохранения результатов калбировки.
        /// С добавление записей в базу
        /// </summary>
        [TestMethod]
        public void SaveCalibrationResultTest()
        {
            _testDb.CreateDatabase();
            _testDb.InitTestSchema();
            string databaseFile = _testDbFile;
            ILogger logger = new LoggerStub();
            var target = new LocalDatabaseStorage(databaseFile, logger);
            target.Open();
            var data = new CalibrationResult
                           {
                               Clusters = new List<AccelData>
                                              {
                                                  new AccelData { Date = DateTime.Now,Ax = 10.0, Ay = 20.0},
                                                  new AccelData { Date = DateTime.Now,Ax = 20.0, Ay = 30.0}
                                              },
                               Parameters = new AccelParams { Date = DateTime.Now}
                           };
            
            target.SaveCalibrationResult(data);

            Assert.AreEqual(_testDb.GetIdCount("accel_params"), 1, "проверяем, что калибровочные параметры записаны");
            Assert.AreEqual(_testDb.GetIdCount("accel_data"), 2, "проверяем, что данные акселерометров записаны");
            Assert.AreEqual(_testDb.GetIdCount("calibr_result"), 2, "проверяем, что результаты калибровки записаны");

            target.Close();
        }

        /// <summary>
        /// Тест сохранения результатов калбировки.
        /// Без добавления в базу параметра и кластеров.
        /// </summary>
        [TestMethod]
        public void SaveCalibrationResultWithoutTest()
        {
            _testDb.CreateDatabase();
            _testDb.InitTestSchema();
            _testDb.PopulateTestAccelData();
            _testDb.PopulateTestAccelParams();
            _testDb.PopulateTestCalibrationResult();
            string databaseFile = _testDbFile;
            ILogger logger = new LoggerStub();
            var target = new LocalDatabaseStorage(databaseFile, logger);
            target.Open();
            var data = new CalibrationResult
            {
                Clusters = new List<AccelData>
                                              {
                                                  new AccelData { Id = 1,Date = DateTime.Now,Ax = 10.0, Ay = 20.0},
                                                  new AccelData { Id = 2,Date = DateTime.Now,Ax = 20.0, Ay = 30.0}
                                              },
                Parameters = new AccelParams { Id = 1,Date = DateTime.Now }
            };

            target.SaveCalibrationResult(data);

            Assert.AreEqual(_testDb.GetIdCount("accel_params"), 3, "проверяем, что калибровочные параметры не добавлены");
            Assert.AreEqual(_testDb.GetIdCount("accel_data"), 3, "проверяем, что данные акселерометров не добавлены");
            Assert.AreEqual(_testDb.GetIdCount("calibr_result"), 2, "проверяем, что результаты калибровки записаны заново");

            target.Close();
        }
        #endregion

        #region GetCalibrationResultByParams

        /// <summary>
        /// Тест выборки результатов калбировки.
        /// </summary>
        [TestMethod]
        public void GetCalibrationResultByParamsTest()
        {
            _testDb.CreateDatabase();
            _testDb.InitTestSchema();
            _testDb.PopulateTestAccelData();
            _testDb.PopulateTestAccelParams();
            _testDb.PopulateTestCalibrationResult();
            string databaseFile = _testDbFile;
            ILogger logger = new LoggerStub();
            var target = new LocalDatabaseStorage(databaseFile, logger);
            target.Open();
            var parameters = new AccelParams {Id = 1};

            var actual = target.GetCalibrationResultByParams(parameters);

            Assert.AreEqual(2,actual.Clusters.Count);

            target.Close();
        }

        #endregion
    }
}

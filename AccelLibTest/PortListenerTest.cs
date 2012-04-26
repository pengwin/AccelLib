using System;
using System.IO;
using System.Threading;
using AccelLib.Models;
using AccelLibTest.Stubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AccelLib;

namespace AccelLibTest
{

    [TestClass]
    public class PortListenerTest
    {

        /// <summary>
        /// Тест конструтора
        /// </summary>
        [TestMethod]
        public void ConstructorTest()
        {
            var portMock = new Mock<IBinaryPort>();
            var loggerStub = new LoggerStub();

            var target = new PortListener(portMock.Object, loggerStub);
            Assert.IsFalse(target.IsListen, "конструктор не запускает поток");
        }

        #region Start/Stop test

        /// <summary>
        /// Тест запуска
        /// </summary>
        [TestMethod]
        public void StartListenTest()
        {
            var portMock = new Mock<IBinaryPort>();
            portMock.Setup(foo => foo.ReadBytesBlock(1)).Returns(new byte[] {0xA}); // возвращаем сигнатуру
            portMock.Setup(foo => foo.ReadBytesBlock(16)).Returns(new byte[]
                                                                      {
                                                                          0, 0, 0, 0, 0, 0, 52, 64, 0, 0, 0, 0, 0, 192, 82,
                                                                          192
                                                                      }); // возвращаем тело пакета
            var loggerStub = new LoggerStub();

            var target = new PortListener(portMock.Object, loggerStub);

            target.StartListen();
            Assert.IsTrue(target.IsListen, "поток запущен");
            target.StopListen();
        }

        /// <summary>
        /// Тест остановки
        /// </summary>
        [TestMethod]
        public void StopListenTest()
        {
            var portMock = new Mock<IBinaryPort>();
            portMock.Setup(foo => foo.ReadBytesBlock(1)).Returns(new byte[] {0xA}); // возвращаем сигнатуру
            portMock.Setup(foo => foo.ReadBytesBlock(16)).Returns(new byte[]
                                                                      {
                                                                          0, 0, 0, 0, 0, 0, 52, 64, 0, 0, 0, 0, 0, 192, 82,
                                                                          192
                                                                      }); // возвращаем тело пакета
            var loggerStub = new LoggerStub();

            var target = new PortListener(portMock.Object, loggerStub);
            target.StartListen();
            target.StopListen();
            Assert.IsFalse(target.IsListen, "поток остановлен");
        }

        #endregion

        #region Exception test

        /// <summary>
        /// Тест неожидаемого исключения при чтении пакетов
        /// </summary>
        [TestMethod]
        public void UnexpectedExceptionTest()
        {
            var portMock = new Mock<IBinaryPort>();
            // выбрасываем неожидаемое потоком исключение
            portMock.Setup(foo => foo.ReadBytesBlock(1)).Throws<NullReferenceException>();
            var loggerStub = new LoggerStub();

            var target = new PortListener(portMock.Object, loggerStub);
            var syncEvent = new AutoResetEvent(false);
            Exception actual = null;
            target.ExceptionCought += (sender, e) =>
                                          {
                                              actual = e.Ex;
                                              syncEvent.Set();
                                          };

            target.StartListen();
            if (!syncEvent.WaitOne(5000))
            {
                Assert.Fail("Таймаут ожидания исключения");
            }

            Assert.IsFalse(target.IsListen, "поток остановлен");
            Assert.IsNotNull(actual as NullReferenceException, "передано правильно исключение");
            target.StopListen();
        }

        /// <summary>
        /// Тест показывает зачем был сделан отлов исключений.
        /// </summary>
        [TestMethod]
        public void ExceptionWorkTest()
        {
            var portMock = new Mock<IBinaryPort>();
            // выбрасываем исключение, например при чтении из закрытого порта
            portMock.Setup(foo => foo.ReadBytesBlock(1)).Throws<IOException>();
            var loggerStub = new LoggerStub();

            var target = new PortListener(portMock.Object, loggerStub);
            var syncEvent = new AutoResetEvent(false);
            EventHandler<ExceptionCoughtArgs> exceptionHandler = (sender, e) => syncEvent.Set();
            target.ExceptionCought += exceptionHandler;

            target.StartListen();
            if (!syncEvent.WaitOne(5000)) // ждем вызова обработчика
            {
                Assert.Fail("Таймаут ожидания исключения");
            }
            target.ExceptionCought -= exceptionHandler;

            // для примера: вызывающий код обработал исключение, открыл порт с валидными данными
            portMock.Setup(foo => foo.ReadBytesBlock(1)).Returns(new byte[] { 0xA });
            portMock.Setup(foo => foo.ReadBytesBlock(16)).Returns(new byte[] { 0, 0, 0, 0, 0, 0, 52, 64, 0, 0, 0, 0, 0, 192, 82, 192 });
            target.AccelDataReceived += (sender, args) => syncEvent.Set();
            target.StartListen(); // снова запускаем

            if (!syncEvent.WaitOne(5000)) // ждем вызова обработчика
            {
                Assert.Fail("Таймаут ожидания данных");
            }

            Assert.IsTrue(target.IsListen, "поток работает");
            target.StopListen();
        }

        /// <summary>
        /// Тест таймаута получения пакетов
        /// </summary>
        [TestMethod]
        public void TimeoutTest()
        {
            var portMock = new Mock<IBinaryPort>();
            // выбрасываем исключение ожидаемое потоком
            portMock.Setup(foo => foo.ReadBytesBlock(1)).Throws<TimeoutException>();
            var silentLogger = new Mock<ILogger>();

            var target = new PortListener(portMock.Object, silentLogger.Object);
            target.StartListen();

            Assert.IsTrue(target.IsListen, "поток не был остановлен");
            target.StopListen();
        }

        #endregion

        /// <summary>
        /// Тест получения пакетов
        /// </summary>
        [TestMethod]
        public void DataReceivingTest()
        {
            var portMock = new Mock<IBinaryPort>();
            portMock.Setup(foo => foo.ReadBytesBlock(1)).Returns(new byte[] {0xA}); // возвращаем сигнатуру
            // возвращаем тело пакета
            portMock.Setup(foo => foo.ReadBytesBlock(16)).Returns(new byte[]{0, 0, 0, 0, 0, 0, 52, 64, 0, 0, 0, 0, 0, 192, 82,192}); 
            var loggerStub = new LoggerStub();

            var target = new PortListener(portMock.Object, loggerStub);

            var expected = new AccelData(20.0, -75.0);
            var syncEvent = new AutoResetEvent(false);
            AccelData actual = null;

            target.AccelDataReceived += (sender, dataArgs) =>
                                       {
                                           syncEvent.Set();
                                           actual = dataArgs.Data;
                                       };

            target.StartListen();
            if (!syncEvent.WaitOne(5000)) // ждем события
            {
                Assert.Fail("Таймаут ожидания данных");
            }
            target.StopListen();

            Assert.AreEqual(expected.Ax, actual.Ax); // проверяем правильность разбора пакета
        }

    }
}

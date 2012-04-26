using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using AccelLib.Models;

namespace AccelLib
{
    public class DataReceivedArgs<T> : EventArgs where T: IPacket
    {
        public T Data { get; private set; }

        public DataReceivedArgs(T data)
        {
            Data = data;
        }
    }

    public class ExceptionCoughtArgs : EventArgs
    {
        public Exception Ex { get; private set; }

        public ExceptionCoughtArgs(Exception ex)
        {
            Ex = ex;
        }
    }

    /// <summary>
    /// 1) Читает данные из порта
    /// 2) Разбирает полученные данные на сообщения
    /// 3) Оповещает подписчиков о получении сообщений
    /// 4) Оповещает подписчиков о произошедших исключениях
    /// Вызов обработчиков - синхронный. 
    /// Обеспечение асинхронности вызовов - задача подписчиков.
    /// </summary>
    public class PortListener
    {

        #region Private fields

        private readonly IBinaryPort _port;
        private readonly ILogger _logger;
        private Thread _worker;
        private bool _continue = false;
        private int _timeout = 1000;
        private delegate void DataReceivedInvoker(IPacket packet);
        private readonly Dictionary<PacketSignatures, DataReceivedInvoker> _eventInvokers;

        #endregion

        /// <summary>
        /// Возвращает состояние просшуивания порта
        /// </summary>
        public bool IsListen { get; private set; }

        #region Events

        #region AccelDataReceived Event

        /// <summary>
        /// Событие: получены данные акселерометра
        /// </summary>
        public event EventHandler<DataReceivedArgs<AccelData>> AccelDataReceived;

        private void AccelDataReceivedInvoker(IPacket packet)
        {
           
            if (AccelDataReceived != null)
            {
                AccelDataReceived(this,new DataReceivedArgs<AccelData>((AccelData)packet));
            }
        }

        #endregion

        #region AccelParamsReceived Event
        /// <summary>
        /// Событие: получены данные калибровки
        /// </summary>
        public event EventHandler<DataReceivedArgs<AccelParams>> AccelParamsReceived;

        private void AccelParamsReceivedInvoker(IPacket packet)
        {

            if (AccelParamsReceived != null)
            {
                AccelParamsReceived(this, new DataReceivedArgs<AccelParams>((AccelParams)packet));
            }
        }
        #endregion

        #region ExceptionCought Event
        /// <summary>
        /// Событие: необрабатываемое исключение
        /// </summary>
        public event EventHandler<ExceptionCoughtArgs> ExceptionCought;

        private void ExceptionCoughtInvoker(Exception ex)
        {
            if (ExceptionCought != null)
            {
                ExceptionCought(this,new ExceptionCoughtArgs(ex));
            }
        }
        #endregion

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="port">прослушиваемый порт</param>
        /// <param name="logger">логгер событий</param>
        /// <param name="dataEndianness">порядок байт в пакетах</param>
        public PortListener(IBinaryPort port, ILogger logger)
        {
            _port = port;
            _logger = logger;
            _eventInvokers = new Dictionary<PacketSignatures, DataReceivedInvoker>();
            _eventInvokers.Add(PacketSignatures.AccelData, AccelDataReceivedInvoker);
            _eventInvokers.Add(PacketSignatures.AccelParams, AccelParamsReceivedInvoker);
        }

        #endregion

        /// <summary>
        /// Инстанцирует экземпляр пакета по сигнатуре
        /// </summary>
        /// <param name="sign"></param>
        /// <returns></returns>
        private IPacket GetPacketBySign(byte sign)
        {
            var signature = (PacketSignatures)sign;
            switch (signature)
            {
                case  PacketSignatures.AccelData:
                    return new AccelData();
                case PacketSignatures.AccelParams:
                    return new AccelParams();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Тело рабочего потока
        /// </summary>
        private void DoWork()
        {
            while (_continue)
            {
                try
                {
                    byte[] sinature = _port.ReadBytesBlock(1); //читаем сигнатуру
                    _logger.Log(String.Format("Получена сигнатура 0x{0:X}", sinature[0]), LogLevel.Debug);

                    var packet = GetPacketBySign(sinature[0]);
                    // если сигнатура не совпадает ни с одной из известных
                    if (packet == null)
                    {
                        _logger.Log(String.Format("Неизвестная сигнатура 0x{0:X}", sinature[0]), LogLevel.Error);
                        continue;
                    }

                    // читаем тело пакета
                    byte[] body = _port.ReadBytesBlock(packet.Size);
                    var buffer = new DataBuffer(Endianness.LittleEndian);
                    buffer.WriteRawData(body);

                    // разбираем данные
                    packet.FromBuffer(buffer);
                    
                    //запоминаем дату получения пакета
                    packet.Date = DateTime.Now;

                    // вызываем обработчик события
                    if (!_eventInvokers.ContainsKey(packet.Signature)) throw new ArgumentException(String.Format("Для сигнатуры {0} не зарегистрированы обработчики", packet.Signature));
                    var invoker = _eventInvokers[packet.Signature];
                    invoker(packet);

                }
                catch (TimeoutException) // ожидаемое исключение, например при незапитанном устройстве
                {
                    _logger.Log(String.Format("Таймаут порта {0}", _port.PortName), LogLevel.Warning);
                }
                catch (Exception ex) // не ожидаемое исключение
                {
                    _logger.Log(String.Format("Исключение: {0}", ex), LogLevel.Error); // пишем в лог
                    _continue = false; // завершаем поток
                    ExceptionCoughtInvoker(ex);
                }
            }
            IsListen = false;
        }


        /// <summary>
        /// Запустить
        /// </summary>
        public void StartListen()
        {
            if ((_worker == null) || (!_worker.IsAlive))
            {
                _worker = new Thread(DoWork);
                _continue = true;
                _worker.Start();
                IsListen = true;
                _logger.Log("Поток успешно запущен", LogLevel.Info);
            }
        }

        /// <summary>
        /// Остановить
        /// </summary>
        public void StopListen()
        {
            if ((_worker != null) && (_worker.IsAlive))
            {
                _continue = false;
                if (!_worker.Join(_timeout))
                {
                    _worker.Abort();
                }
                IsListen = false;
                _logger.Log("Поток успешно остановлен", LogLevel.Info);
            }
        }

    }
}

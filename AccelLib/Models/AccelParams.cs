using System;

namespace AccelLib.Models
{
    /// <summary>
    /// Калибровочные параметры акселерометра
    /// </summary>
    public class AccelParams : IPacket
    {
        private const int _size = 34;

        public PacketSignatures Signature { get { return PacketSignatures.AccelParams; } }
        public int Size { get { return _size; } }

        /// <summary>
        /// Ключ в базе данных
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Номер акселерометра
        /// </summary>
        public UInt16 SensorNumber { get; set; }

        /// <summary>
        /// Смещение нуля по Оси X
        /// </summary>
        public double OffsetX { get; set; }

        /// <summary>
        /// Смещение нуля по Оси Y
        /// </summary>
        public double OffsetY { get; set; }

        /// <summary>
        /// Кажущееся ускоренеи свободного падения по оси X
        /// </summary>
        public double GravityX { get; set; }

        /// <summary>
        /// Кажущееся ускоренеи свободного падения по оси Y
        /// </summary>
        public double GravityY { get; set; }

        /// <summary>
        /// Дата когда получены данные
        /// </summary>
        public DateTime Date { get; set; }

        #region Чтение/запись в буфер

        /// <summary>
        /// Читает значения полей из буфера
        /// </summary>
        /// <param name="buffer">буфер бинарных данных</param>
        public void FromBuffer(DataBuffer buffer)
        {
            SensorNumber = buffer.ReadUInt16();
            OffsetX = buffer.ReadDouble();
            OffsetY = buffer.ReadDouble();
            GravityX = buffer.ReadDouble();
            GravityY = buffer.ReadDouble();
        }

        /// <summary>
        /// Записывает значения полей в буфер
        /// </summary>
        /// <param name="buffer"></param>
        public void ToBuffer(DataBuffer buffer)
        {
            buffer.WriteUInt16(SensorNumber);
            buffer.WriteDouble(OffsetX);
            buffer.WriteDouble(OffsetY);
            buffer.WriteDouble(GravityX);
            buffer.WriteDouble(GravityY);
        }

        #endregion

        public override string ToString()
        {
            return String.Format("SensorNumber: {0} OffsetX: {1:0.00} OffsetX: {2:0.00} gX: {3:0.00} gY: {4:0.00}", 
                                 SensorNumber, OffsetX, OffsetY, GravityX, GravityY);
        }

    }
}

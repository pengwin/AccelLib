using System;

namespace AccelLib.Models
{

    /// <summary>
    /// Показания акселерометра
    /// </summary>
    public class AccelData : IPacket
    {
        private const int _size = 16;

        public PacketSignatures Signature { get { return PacketSignatures.AccelData; } }
        public int Size { get { return _size;  } }

        /// <summary>
        /// Ключ в базе данных
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Дата когда получены данные
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Показания оси X
        /// </summary>
        public double Ax { get; set; }

        /// <summary>
        /// Показания оси Y
        /// </summary>
        public double Ay { get; set; }

        #region Конструкторы

        /// <summary>
        /// Конструктор
        /// </summary>
        public AccelData()
        {
            Date = new DateTime();
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="ax">показания оси X</param>
        /// <param name="ay">показания оси Y</param>
        public AccelData(double ax, double ay) : this()
        {
            Ax = ax;
            Ay = ay;
        }

        #endregion

        #region Мат. операторы

        public static AccelData operator + (AccelData a1,AccelData a2)
        {
            return new AccelData(a1.Ax+a2.Ax,a1.Ay+a2.Ay);       
        }

        public static AccelData operator -(AccelData a1, AccelData a2)
        {
            return new AccelData(a1.Ax - a2.Ax, a1.Ay - a2.Ay);
        }

        public static AccelData operator /(AccelData a1, double a2)
        {
            return new AccelData(a1.Ax/a2, a1.Ay/a2);
        }

        #endregion

        #region Чтение/запись в буфер

        /// <summary>
        /// Читает значения полей из буфера
        /// </summary>
        /// <param name="buffer">буфер бинарных данных</param>
        public void FromBuffer(DataBuffer buffer)
        {
            Ax = buffer.ReadDouble();
            Ay = buffer.ReadDouble();
        }

        /// <summary>
        /// Записывает значения полей в буфер
        /// </summary>
        /// <param name="buffer"></param>
        public void ToBuffer(DataBuffer buffer)
        {
            buffer.WriteDouble(Ax);
            buffer.WriteDouble(Ay);
        }

        #endregion

        public override string ToString()
        {
            return String.Format("Ax: {0:0.00} Ay: {1:0.00}", Ax, Ay);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace AccelLib
{
    /// <summary>
    /// Порядок байт в слове
    /// </summary>
    public enum Endianness
    {
        /// <summary>
        /// Silicon, MicroBlaze
        /// </summary>
        BigEndian, 
        /// <summary>
        /// ARM,x86
        /// </summary>
        LittleEndian, 
    }

    /// <summary>
    /// Класс отвечающий за:
    /// 1) Запись данных, с необходимым порядком байт в слове, в массив байтов
    /// 2) Чтение данных, с необходимым порядком байт в слове, из массива байтов
    /// </summary>
    public class DataBuffer : ICloneable
    {
        /// <summary>
        /// Порядок байт в слове в принимаемых данных
        /// </summary>
        public Endianness ByteOrder { private set; get; }
        List<byte> _data; // буффер данных
        int _readPos; // позиция курсора чтения

        /// <summary>
        /// Кол-во непрочитанных данных
        /// </summary>
        public int UnreadedData
        {
            get
            {
                return (_data.Count - _readPos);
            }
        }

        /// <summary>
        /// Сырые данные
        /// </summary>
        public byte[] RawData
        {
            get
            {
                return _data.ToArray();
            }
        }

        /// <summary>
        /// Очищаем буффер приянтых байт
        /// </summary>
        public void ClearBuffer()
        {
            _readPos = 0;
            _data.Clear();
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="byteOrder"></param>
        public DataBuffer(Endianness byteOrder)
        {
            this.ByteOrder = byteOrder;
            _data = new List<byte>();
            _readPos = 0;
        }

        #region Методы для чтения

        /// <summary>
        /// Читаем буффер данных для преобразования
        /// Если надо переворачиваем порядок байт
        /// </summary>
        /// <param name="dataSize">размер читаемых данных</param>
        /// <returns></returns>
        byte[] ReadDataBuffer(int dataSize)
        {
            byte[] temp = new byte[dataSize];
            _data.CopyTo(_readPos, temp, 0, dataSize); // копируем байты 
            if (ByteOrder == Endianness.BigEndian)
            {
                temp = temp.Reverse().ToArray(); // переворачиваем байты в буффере
            }
            _readPos += dataSize;
            return temp;
        }

        /// <summary>
        /// Читаем uchar из буффера
        /// Без контроля границ буффера !!!!!!!!!!!!!
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            byte result = _data[_readPos];
            _readPos++;
            return result;
        }

        /// <summary>
        /// Читаем ushort из буффера
        /// Без контроля границ буффера !!!!!!!!!!!!!
        /// </summary>
        /// <returns></returns>
        public UInt16 ReadUInt16()
        {
            UInt16 result = 0;

            byte[] temp = ReadDataBuffer(2);

            result = BitConverter.ToUInt16(temp, 0); //получаем значение

            return result;
        }

        /// <summary>
        /// Читаем short из буффера
        /// Без контроля границ буффера !!!!!!!!!!!!!
        /// </summary>
        /// <returns></returns>
        public Int16 ReadInt16()
        {
            Int16 result = 0;

            byte[] temp = ReadDataBuffer(2);

            result = BitConverter.ToInt16(temp, 0); //получаем значение

            return result;
        }

        /// <summary>
        /// Читаем ulong из буффера
        /// Без контроля границ буффера !!!!!!!!!!!!!
        /// </summary>
        /// <returns></returns>
        public UInt32 ReadUInt32()
        {
            UInt32 result = 0;

            byte[] temp = ReadDataBuffer(4);

            result = BitConverter.ToUInt32(temp, 0); //получаем значение

            return result;
        }

        /// <summary>
        /// Читаем uint64 из буффера
        /// Без контроля границ буффера !!!!!!!!!!!!!
        /// </summary>
        /// <returns></returns>
        public UInt64 ReadUInt64()
        {
            UInt64 result = 0;

            byte[] temp = ReadDataBuffer(8);

            result = BitConverter.ToUInt64(temp, 0); //получаем значение

            return result;
        }

        /// <summary>
        /// Читаем ulong из буффера
        /// Без контроля границ буффера !!!!!!!!!!!!!
        /// </summary>
        /// <returns></returns>
        public Int32 ReadInt32()
        {
            Int32 result = 0;

            byte[] temp = ReadDataBuffer(4);

            result = BitConverter.ToInt32(temp, 0); //получаем значение

            return result;
        }

        /// <summary>
        /// Читаем float из буффера
        /// Без контроля границ буффера !!!!!!!!!!!!!
        /// </summary>
        /// <returns></returns>
        public float ReadSingle()
        {
            float result = 0;

            byte[] temp = ReadDataBuffer(4);

            result = BitConverter.ToSingle(temp, 0); //получаем значение

            return result;
        }

        // <summary>
        /// Читаем double из буффера
        /// Без контроля границ буффера !!!!!!!!!!!!!
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            double result = 0;

            byte[] temp = ReadDataBuffer(8);

            result = BitConverter.ToDouble(temp, 0); //получаем значение

            return result;
        }

         /// <summary>
        /// Читаем данные из буффера как есть
        ///  Без контроля границ буффера !!!!!!!!!!!!!
        /// </summary>
        /// <returns></returns>
        public byte[] ReadRawData(int count)
        {
            byte[] result = new byte[count];
            _data.CopyTo(_readPos, result, 0, count);
            _readPos += count;
            return result;
        }

        #endregion

        #region Методы для записи

        /// <summary>
        /// Пишем данные в буффер
        /// если надо переворачиваем порядок
        /// </summary>
        /// <param name="data"></param>
        void WriteDataBuffer(byte[] data)
        {
            byte[] temp = new byte[data.Length];
            // копируем данные на всякий случай
            data.CopyTo(temp, 0);
            if (ByteOrder == Endianness.BigEndian)
            {
                temp = temp.Reverse().ToArray(); // переворачиваем байты в буффере
            }
            _data.AddRange(temp);
        }

        /// <summary>
        /// Пишем байт в буффер 
        /// </summary>
        /// <param name="data"></param>
        public void WriteByte(byte data)
        {
            _data.Add(data);
        }

        /// <summary>
        /// Пишем ushort в буффер
        /// </summary>
        /// <param name="data"></param>
        public void WriteInt32(Int32 data)
        {
            byte[] temp = BitConverter.GetBytes(data);
            WriteDataBuffer(temp);
        }

        /// <summary>
        /// Пишем ushort в буффер
        /// </summary>
        /// <param name="data"></param>
        public void WriteUInt16(UInt16 data)
        {
            byte[] temp = BitConverter.GetBytes(data);
            WriteDataBuffer(temp);
        }

        /// <summary>
        /// Пишем ushort в буффер
        /// </summary>
        /// <param name="data"></param>
        public void WriteUInt32(UInt32 data)
        {
            byte[] temp = BitConverter.GetBytes(data);
            WriteDataBuffer(temp);
        }

        /// <summary>
        /// Пишем uint64 в буффер
        /// </summary>
        /// <param name="data"></param>
        public void WriteUInt64(UInt64 data)
        {
            byte[] temp = BitConverter.GetBytes(data);
            WriteDataBuffer(temp);
        }

        /// <summary>
        /// Пишем float в буффер
        /// </summary>
        /// <param name="data"></param>
        public void WriteSingle(float data)
        {
            byte[] temp = BitConverter.GetBytes(data);
            WriteDataBuffer(temp);
        }

        /// <summary>
        /// Пишем double в буффер
        /// </summary>
        /// <param name="data"></param>
        public void WriteDouble(double data)
        {
            byte[] temp = BitConverter.GetBytes(data);
            WriteDataBuffer(temp);
        }

        /// <summary>
        /// Пишем данные в буфер как есть
        /// </summary>
        /// <param name="data"></param>
        public void WriteRawData(byte[] data)
        {
            _data.AddRange(data);
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            DataBuffer buffer = new DataBuffer(this.ByteOrder);
            buffer.WriteRawData(this.RawData);
            return buffer;
        }

        #endregion
    }
}

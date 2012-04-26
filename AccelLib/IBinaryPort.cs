using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AccelLib
{
    /// <summary>
    /// Порт устройства работающего с  бинарными данными
    /// </summary>
    public interface IBinaryPort
    {

        string PortName { get; }
        void Open();
        void Close();
        /// <summary>
        /// Читает байты из порта, блокирует вызывающий поток
        /// </summary>
        /// <param name="count">кол-во читаемых байт</param>
        /// <returns>прочитанные байты</returns>
        byte[] ReadBytesBlock(int count);
        void WriteBytes(byte[] bytes);
    }
}

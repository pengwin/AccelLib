using System;
using AccelLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AccelLibTest
{
    [TestClass]
    public class DataBufferTest
    {
        /// <summary>
        /// Тест передачи порядка байт в конструкторе
        /// </summary>
        [TestMethod]
        public void ConstructorTest()
        {
            var endianess = Endianness.BigEndian;
            DataBuffer target = new DataBuffer(endianess);
            Assert.AreEqual(endianess,target.ByteOrder);
        }

        /// <summary>
        /// Тест записи сырых данных
        /// </summary>
        [TestMethod]
        public void WriteRawDataTest()
        {
            var endianess = Endianness.BigEndian;
            DataBuffer target = new DataBuffer(endianess);

            var data = new byte[10];
            target.WriteRawData(data);

            Assert.AreEqual(data.Length, target.UnreadedData);
            CollectionAssert.AreEqual(data, target.RawData);
        }

        #region UInt32 test

        /// <summary>
        /// Тест записи uint32 (big endian)
        /// </summary>
        [TestMethod]
        public void WriteUInt32Test_BigEndian()
        {
            var endianess = Endianness.BigEndian;
            DataBuffer target = new DataBuffer(endianess);

            var expected = new byte[]{ 0xFF, 0xAA, 0xBB, 0x22};
            target.WriteUInt32(0xFFAABB22);

            var actual = target.RawData;

            Assert.AreEqual(expected.Length, target.UnreadedData);
            CollectionAssert.AreEqual(expected,actual);
        }

        /// <summary>
        /// Тест записи uint32 (little endian)
        /// </summary>
        [TestMethod]
        public void WriteUInt32Test_LittleEndian()
        {
            var endianess = Endianness.LittleEndian;
            DataBuffer target = new DataBuffer(endianess);

            var expected = new byte[] { 0x22, 0xBB, 0xAA, 0xFF };
            target.WriteUInt32(0xFFAABB22);

            Assert.AreEqual(expected.Length, target.UnreadedData);
            CollectionAssert.AreEqual(expected, target.RawData);
        }

        /// <summary>
        /// Тест чтения uint32 (little endian)
        /// </summary>
        [TestMethod]
        public void ReadUInt32Test_LittleEndian()
        {
            var endianess = Endianness.LittleEndian;
            DataBuffer target = new DataBuffer(endianess);

            UInt32 expected = 0xFFAABB22;

            var data = new byte[] { 0x22, 0xBB, 0xAA, 0xFF };
            target.WriteRawData(data);
            var actual = target.ReadUInt32();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Тест чтения uint32 (big endian)
        /// </summary>
        [TestMethod]
        public void ReadUInt32Test_BigEndian()
        {
            var endianess = Endianness.BigEndian;
            DataBuffer target = new DataBuffer(endianess);

            UInt32 expected = 0xFFAABB22;

            var data = new byte[] { 0xFF, 0xAA, 0xBB, 0x22 };
            target.WriteRawData(data);
            var actual = target.ReadUInt32();

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Double test

        /// <summary>
        /// Тест чтения double (LittleEndian)
        /// </summary>
        [TestMethod]
        public void ReadDoubleTest_LittleEndian()
        {
            var endianess = Endianness.LittleEndian;
            DataBuffer target = new DataBuffer(endianess);

            double expected = 365784e40;
            var expectedBytes = BitConverter.GetBytes(expected);
            target.WriteRawData(expectedBytes);

            var actual = target.ReadDouble();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Тест записи double (LittleEndian)
        /// </summary>
        [TestMethod]
        public void WriteDoubleTest_LittleEndian()
        {
            var endianess = Endianness.LittleEndian;
            DataBuffer target = new DataBuffer(endianess);

            double data = 365784e40;
            var expected = BitConverter.GetBytes(data);

            target.WriteDouble(data);
            var actual = target.RawData;

            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion
    }
}

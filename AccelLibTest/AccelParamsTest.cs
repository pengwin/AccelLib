using AccelLib;
using AccelLib.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AccelLibTest
{
    [TestClass]
    public class AccelParamsTest
    {
        #region Buffer operations tets

        /// <summary>
        /// Тест чтения из буфера
        /// </summary>
        [TestMethod]
        public void FromBufferTest()
        {
            var target = new AccelParams();

            var expected = new AccelParams
                               {
                                   SensorNumber = 1,
                                   OffsetX = 20.0,
                                   OffsetY = -75.0,
                                   GravityX = -20.0,
                                   GravityY = 75.0
                               };

            var buffer = new DataBuffer(Endianness.LittleEndian);
            buffer.WriteUInt16(1);
            buffer.WriteDouble(20.0);
            buffer.WriteDouble(-75.0);
            buffer.WriteDouble(-20.0);
            buffer.WriteDouble(75.0);

            target.FromBuffer(buffer);

            var tt = new DataBuffer(Endianness.LittleEndian);
            target.ToBuffer(tt);

            Assert.AreEqual(expected.SensorNumber,target.SensorNumber);
            Assert.AreEqual(expected.OffsetX, target.OffsetX);
            Assert.AreEqual(expected.OffsetY, target.OffsetY);
            Assert.AreEqual(expected.GravityX, target.GravityX);
            Assert.AreEqual(expected.GravityY, target.GravityY);
        }

        /// <summary>
        /// Тест записи в буфер
        /// </summary>
        [TestMethod]
        public void ToBufferTest()
        {
            var target = new AccelParams
                             {
                                 SensorNumber = 1,
                                 OffsetX = 20.0,
                                 OffsetY = -75.0,
                                 GravityX = 20.0,
                                 GravityY = -75.0
                             };

            var expected = new byte[] { 1,0,0, 0, 0, 0, 0, 0, 52, 64, 0, 0, 0, 0, 0, 192, 82, 192, 0, 0, 0, 0, 0, 0, 52, 64, 0, 0, 0, 0, 0, 192, 82, 192 };

            var buffer = new DataBuffer(Endianness.LittleEndian);

            target.ToBuffer(buffer);

            CollectionAssert.AreEqual(expected, buffer.RawData);
        }

        #endregion
    }
}

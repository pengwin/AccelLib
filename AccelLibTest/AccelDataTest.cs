using AccelLib;
using AccelLib.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AccelLibTest
{
    [TestClass]
    public class AccelDataTest
    {
        #region Contructor test

        /// <summary>
        /// Тест конструктора без параметров
        /// </summary>
        [TestMethod]
        public void ConstructorWihoutParametersTest()
        {
            var target = new AccelData();

            Assert.AreEqual(0F,target.Ax);
            Assert.AreEqual(0F,target.Ay );
        }

        /// <summary>
        /// Тест конструктора с параметрами
        /// </summary>
        [TestMethod]
        public void ConstructorWithParametersTest()
        {
            double ax = 10.0;
            double ay = 50.0;
            var target = new AccelData(ax,ay);

            Assert.AreEqual(ax, target.Ax);
            Assert.AreEqual(ay, target.Ay);
        }


       #endregion

        #region Math operators test

        /// <summary>
        /// Тест оператора сложения
        /// </summary>
        [TestMethod]
        public void OperatorPlusTest()
        {
            var target1 = new AccelData(15.0, 75.0);
            var target2 = new AccelData(5.0, -150.0);

            var expected = new AccelData(20.0, -75.0);

            var actual = target1 + target2;

            Assert.AreEqual(expected.Ax, actual.Ax);
            Assert.AreEqual(expected.Ay, actual.Ay);
        }

        /// <summary>
        /// Тест оператора вычитания
        /// </summary>
        [TestMethod]
        public void OperatorMinusTest()
        {
            var target1 = new AccelData(15.0, 75.0);
            var target2 = new AccelData(5.0, -150.0);

            var expected = new AccelData(10.0, 225.0);

            var actual = target1 - target2;

            Assert.AreEqual(expected.Ax, actual.Ax);
            Assert.AreEqual(expected.Ay, actual.Ay);
        }

        /// <summary>
        /// Тест деления на число
        /// </summary>
        [TestMethod]
        public void OperatorDivideTest()
        {
            var target = new AccelData(15.0, 75.0);
            var divizor = 5.0;

            var expected = new AccelData(3.0, 15.0);

            var actual = target/divizor;

            Assert.AreEqual(expected.Ax, actual.Ax);
            Assert.AreEqual(expected.Ay, actual.Ay);
        }

        #endregion

        #region Buffer operations tets

        /// <summary>
        /// Тест чтения из буфера
        /// </summary>
        [TestMethod]
        public void FromBufferTest()
        {
            var target = new AccelData(15.0, 75.0);

            var expected = new AccelData(20.0, -75.0);

            var buffer = new DataBuffer(Endianness.LittleEndian);
            buffer.WriteDouble(20.0);
            buffer.WriteDouble(-75.0);

            target.FromBuffer(buffer);

            Assert.AreEqual(expected.Ax, target.Ax);
            Assert.AreEqual(expected.Ay, target.Ay);
        }

        /// <summary>
        /// Тест записи в буфер
        /// </summary>
        [TestMethod]
        public void ToBufferTest()
        {
            var target = new AccelData(20.0, -75.0);

            var expected = new byte[] {0, 0, 0, 0, 0, 0,52,64,0,0,0,0,0,192,82,192};

            var buffer = new DataBuffer(Endianness.LittleEndian);

            target.ToBuffer(buffer);

            CollectionAssert.AreEqual(expected, buffer.RawData);
        }

        #endregion

    }
}

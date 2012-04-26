using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AccelLib.Models
{
    /// <summary>
    /// Результаты калибровки.
    /// Связывает посчитанные параметры и набор кластеров.
    /// </summary>
    public class CalibrationResult
    {

        /// <summary>
        /// Вычисленные параметры
        /// </summary>
        public AccelParams Parameters { get; set; }

        /// <summary>
        /// Данные акселерометра использованные при вычислениях
        /// </summary>
        public IList<AccelData> Clusters { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        public CalibrationResult()
        {
            Parameters = new AccelParams();
            Clusters = new List<AccelData>();
        }
    }
}

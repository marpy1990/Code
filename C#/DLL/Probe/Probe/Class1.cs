using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Probe
{
    /// <summary>
    /// 探针接口
    /// </summary>
    public interface Probe
    {
        /// <summary>
        /// 读取一次性能值
        /// </summary>
        /// <returns>性能值集合</returns>
        ICollection<DetectedData> GetValues();
    }

    /// <summary>
    /// 性能数据接口
    /// </summary>
    public class DetectedData
    {
        public string categoryName;
        public string instanceName;
        public object value;
    }
}

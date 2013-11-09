/********************************************************************************
 * Author:  marpy
 * Date:    2013/10/2
 * Description: 采集到的实例数据信息仓，性能采集模块每次采样后的数据会保存在该类
 *              中，包括实例名对应的性能总和以及采样次数。这样数据上传模块就可以
 *              上传一段时间内的平均性能数据
 *              对InstanceValues的访问采用信号量控制，确保同一时间只有一个线程可
 *              以访问实例仓
 ********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MonitorEvent;

namespace MonitorClient
{
    /// <summary>
    /// 实例数据信息仓
    /// </summary>
    class InstanceValues
    {
        #region 性能值类型模块
        /// <summary>
        /// 存储监视项目的性能值
        /// </summary>
        private class DataValue
        {
            /// <summary>
            /// 多次采样的性能之和
            /// </summary>
            private object sum;

            /// <summary>
            /// 采样次数
            /// </summary>
            private int n;

            /// <summary>
            /// 非可求和并取平均类性能
            /// </summary>
            private object value;

            /// <summary>
            /// 是否是可求和类型
            /// </summary>
            private bool isSummable;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="value">性能值</param>
            public DataValue(object value)
            {
                if (value is double || value is float)
                {
                    isSummable = true;
                    sum = value;
                    n = 1;
                    this.value = value;
                }
                else
                {
                    isSummable = false;
                    this.value = value;
                }
            }

            /// <summary>
            /// 添加性能值
            /// </summary>
            /// <param name="value">性能值</param>
            public void Add(object value)
            {
                this.value = value;
                if (isSummable)
                {
                    sum = (dynamic)sum + (dynamic)value;
                    n += 1;
                }
            }

            /// <summary>
            /// 取出性能值
            /// </summary>
            /// <returns>性能值</returns>
            public object GetValue()
            {
                if (isSummable)
                {
                    if (n == 0) return 0;
                    dynamic value = (dynamic)sum / n;
                    sum = 0;
                    n = 0;
                    return value;
                }
                else
                {
                    return this.value;
                }
            }

            /// <summary>
            /// 查看实时性能值
            /// </summary>
            /// <returns></returns>
            public object RealTimeValue()
            {
                return value;
            }

            /// <summary>
            /// 查看性能值
            /// </summary>
            /// <returns>性能值</returns>
            public object Value()
            {
                if (isSummable)
                {
                    try
                    {
                        dynamic value = (dynamic)sum / n;
                        return value;
                    }
                    catch
                    {
                        return -1;
                    }
                }
                else
                {
                    return this.value;
                }
            }

            /// <summary>
            /// 查看是否有性能值
            /// </summary>
            /// <returns>是否存在性能值</returns>
            public bool ContainsValue()
            {
                return !isSummable || n != 0;
            }

        }
        #endregion

        /// <summary>
        /// 实例名称到性能包数据的映射表
        /// </summary>
        private Dictionary<string, DataValue> instanceTable = new Dictionary<string, DataValue>();

        /// <summary>
        /// 实例数据信息仓信号量
        /// </summary>
        private Semaphore instanceTableSem = new Semaphore(1, 1);

        /// <summary>
        /// 向实例仓中添加性能数据
        /// </summary>
        /// <param name="instanceName">实例名称</param>
        /// <param name="value">性能值</param>
        public void Add(string instanceName, object value)
        {
            instanceTableSem.WaitOne();                 //访问信息表控制
            DataValue data;
            if (instanceTable.ContainsKey(instanceName))    //存在实例
            {
                data = instanceTable[instanceName];
                data.Add(value);
            }
            else
            {
                data = new DataValue(value);
                instanceTable.Add(instanceName, data);
            }
            instanceTableSem.Release();                 //释放资源
        }

        /// <summary>
        /// 获取该对象所包含的所有实例名字
        /// </summary>
        public ICollection<string> instanceNames
        {
            get
            {
                instanceTableSem.WaitOne();                 //访问信息表控制
                ICollection<string> names = instanceTable.Keys;
                instanceTableSem.Release();                 //释放资源
                return names;
            }
        }

        /// <summary>
        /// 获取对应实例的性能，若为double类型则会返回这段时间内的平均值，该方法会同时清空对应实例的数据仓
        /// </summary>
        /// <param name="instanceName">实例名</param>
        /// <returns>性能值，失败返回0</returns>
        public object GetValue(string instanceName)
        {
            instanceTableSem.WaitOne();                 //访问信息表控制
            DataValue data = instanceTable[instanceName];
            object value = data.GetValue();
            instanceTableSem.Release();                 //释放资源
            return value;
          
        }

        /// <summary>
        /// 获取对应实例的性能实时值
        /// </summary>
        /// <param name="instanceName">实例名</param>
        /// <returns>实时值</returns>
        public object RealTimeValue(string instanceName)
        {
            DataValue data = instanceTable[instanceName];
            object value = data.RealTimeValue();
            return value;
        }

        /// <summary>
        /// 查看对应实例的性能平均值，该方法不会清空数据仓，测试用
        /// </summary>
        /// <param name="instanceName">实例名</param>
        /// <returns>性能值，失败返回0</returns>
        internal object ViewValue(string instanceName)
        {
            DataValue data = instanceTable[instanceName];
            object value = data.Value();
            return value;
        }

        /// <summary>
        /// 确定对应实例是否含有值
        /// </summary>
        /// <param name="instanceName">实例名</param>
        /// <returns>是否含有值</returns>
        public bool ContainsValue(string instanceName)
        {
            instanceTableSem.WaitOne();                 //访问信息表控制
            bool contains = instanceTable.ContainsKey(instanceName)
                && instanceTable[instanceName].ContainsValue();
            instanceTableSem.Release();                 //释放资源
            return contains;
        }
    }
}

/********************************************************************************
 * Author:  marpy
 * Date:    2013/10/2
 * Description: 监视项目（Category）信息仓，一个监视项目可以对应多个监视实例。
 *              监视项目是配置模块（即可由用户自由配置的模块）可以处理的最小单位，
 *              保存性能采集模块采集到的数据信息。
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorClient
{
    /// <summary>
    /// 监视项目信息仓
    /// </summary>
    class CategoryValues
    {
        /// <summary>
        /// 监视项目到监视实例仓的映射表
        /// </summary>
        private Dictionary<string, InstanceValues> categoryTable = new Dictionary<string, InstanceValues>();

        /// <summary>
        /// 清空监视项目仓
        /// </summary>
        public void Clear()
        {
            categoryTable.Clear();
        }

        /// <summary>
        /// 添加监视项目
        /// </summary>
        /// <param name="categoryName">监视项目名</param>
        public void Add(string categoryName)
        {
            if (!categoryTable.ContainsKey(categoryName))
                categoryTable.Add(categoryName, new InstanceValues());
        }

        /// <summary>
        /// 添加监视项目
        /// </summary>
        /// <param name="categoryNames">监视项目名集合</param>
        public void AddRange(IEnumerable<string> categoryNames)
        {
            foreach (string name in categoryNames)
            {
                this.Add(name);
            }
        }

        /// <summary>
        /// 是否包含对应项目名
        /// </summary>
        /// <param name="categoryName">项目名</param>
        /// <returns>是否包含该项目</returns>
        public bool Contains(string categoryName)
        {
            return categoryTable.ContainsKey(categoryName);
        }

        /// <summary>
        /// 返回指定项目名称的实例仓
        /// </summary>
        /// <param name="categoryName">项目名</param>
        /// <returns>实例仓</returns>
        public InstanceValues this[string categoryName]
        {
            get
            {
                return categoryTable[categoryName];
            }
            set
            {
                categoryTable[categoryName] = value;
            }
        }

        /// <summary>
        /// 所包含的所有监视项目名
        /// </summary>
        public ICollection<string> categoryNames
        {
            get
            {
                return categoryTable.Keys;
            }
        }
    
    }
}

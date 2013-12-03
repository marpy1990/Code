/********************************************************************************
 * Author:  marpy
 * Date:    2013/10/1
 * Description: 性能采集模块，建立探针并采集性能信息。
 *              模块处理器之间并行运行，通过事件驱动和通信。
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using MonitorEvent;

namespace MonitorClient
{
    /// <summary>
    /// 性能采集模块，建立探针并采集性能信息
    /// </summary>
    class PerformanceCollector : Processor
    {
        #region 探针构建模块
        /// <summary>
        /// 性能采集探针
        /// </summary>
        private List<Probe.Probe> probes = new List<Probe.Probe>();

        /// <summary>
        /// 探针构建模块主控方法，建立探针
        /// <param name="sender">发送者</param>
        /// <param name="e">事件数据</param>
        /// </summary>
        private void ProbeBuilding(Processor sender, EventArgs e)
        {
            try
            {
                string[] filenames = LoadProbeBootFile();
                foreach (string probeFile in filenames)
                {
                    LoadProbeFile(probeFile);
                }

                SendEvent(sender, new ProbleBuildedConfirmEvent());  //加载成功，发送确认条给主控模块
            }
            catch
            {
                MessageBox.Show("ProbeBuilding Error");
            }
        }

        /// <summary>
        /// 从名为probeFile的文件加载探针
        /// </summary>
        /// <param name="probeFile">dll文件名</param>
        /// <returns>探针集</returns>
        private void LoadProbeFile(string probeFile)
        {
            try
            {
                //probes.Clear();
                Assembly ass = Assembly.LoadFrom(probeFile);
                Type[] types = ass.GetTypes();
                foreach (Type type in types)
                {
                    if (type.GetInterface(typeof(Probe.Probe).Name) != null)
                    {
                        Object obj = Activator.CreateInstance(type, null);
                        Probe.Probe probe = (Probe.Probe)obj;
                        probes.Add(probe); ;
                    }
                }
            }
            catch
            {
                LoadProbeFileError(probeFile);
            }
        }

        /// <summary>
        /// 从加载探针失败处理，可选
        /// </summary>
        /// <param name="probeFile">探针dll文件名</param>
        private void LoadProbeFileError(string probeFile)
        {
            //可选
            MessageBox.Show("LoadProbeFileError");
        }

        /// <summary>
        /// 加载探针引导文件，读取可用探针dll库名
        /// </summary>
        /// <returns>可用探针dll库名</returns>
        private string[] LoadProbeBootFile()
        {
            try
            {
                FileStream fs = new FileStream(Global.probeBootFile, FileMode.Open);
                StreamReader m_streamReader = new StreamReader(fs);
                string[] filenames = m_streamReader.ReadToEnd().Split(new char[] { '\n' });
                m_streamReader.Close();
                fs.Close();
                return filenames;
            }
            catch (FileNotFoundException)
            {
                LoadProbeBootFileError();
                return null;
            }
        }

        /// <summary>
        /// 探针引导文件加载失败处理，可选
        /// </summary>
        private void LoadProbeBootFileError()
        {
            //可选
            MessageBox.Show("LoadProbeBootFileError");
        }
        #endregion

        #region 性能采集模块
        /// <summary>
        /// 性能采集周期
        /// </summary>
        private TimeSpan collectPeriod = new TimeSpan(0, 0, 0, 0, 300);

        /// <summary>
        /// 性能采集模块的主控方法，通过探针采集性能
        /// </summary>
        private void PerformanceCollect(Processor sender, EventArgs e)
        {
            foreach (Probe.Probe probe in probes)
            {
                try
                {
                    foreach (Probe.DetectedData data in probe.GetValues())
                    {
                        if (!Global.categoryValues.Contains(data.categoryName))
                            Global.categoryValues.Add(data.categoryName);                       
                        Global.categoryValues[data.categoryName].Add(data.instanceName, data.value);
                    }
                }
                catch
                {
                    PerformanceCollectError(probe);
                }
            }
        }

        /// <summary>
        /// 探针失效错误处理
        /// </summary>
        private void PerformanceCollectError(Probe.Probe probe)
        {
            //可选
            MessageBox.Show(probe.ToString());
        }

        /// <summary>
        /// 事件预处理方法，主要针对性能采集事件，并预约下一次采样
        /// </summary>
        /// <param name="e">事件数据</param>
        /// <returns>是否跳过事件处理</returns>
        protected override bool PretreatToSkip(EventArgs e)
        {
            if (e is PerformanceCollectEvent)
            {
                DateTime Now = DateTime.Now;
                PerformanceCollectEvent pe = (PerformanceCollectEvent)e;
                if (pe.sendDate.Add(collectPeriod) <= Now)
                {
                    pe.sendDate = Now;  //到达采样时间，预约下一次采样
                    SendEvent(this, pe);
                    return false;
                }
                else
                {
                    SendEvent(this, pe);
                    return true;        //未达到采样时间，跳过处理线程
                }
            }
            return false;
        }
        #endregion

        #region 处理器构造模块
        /// <summary>
        /// 初始化，绑定时间处理方法
        /// </summary>
        public PerformanceCollector()
        {
            this.eventHandlePeriod = new TimeSpan(0, 0, 0, 0, 100); //事件处理周期为100ms
            eventHandleTable.Add(typeof(ProbeBuildingEvent), this.ProbeBuilding);           //绑定探针构造处理
            eventHandleTable.Add(typeof(PerformanceCollectEvent), this.PerformanceCollect); //绑定性能采集处理
        }
        #endregion
    }
}

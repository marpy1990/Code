/********************************************************************************
 * Author:  marpy
 * Date:    2013/10/5
 * Description: 通信传输流，负责转换事件/网络传输文本
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MonitorEvent
{
    /// <summary>
    /// 标准的通信事件接口，所有通讯事件的都必须使用这个类型
    /// </summary>
    class StandardTransportStreamEvent : EventArgs
    {
        /// <summary>
        /// 事件名，可以是本命名空间内的任意事件
        /// </summary>
        public string eventTypeName;

        /// <summary>
        /// 需要通讯的事件的Json数据
        /// </summary>
        public string eventJsonText;

        public StandardTransportStreamEvent(string typeName, string jsonText)
        {
            this.eventTypeName = typeName;
            this.eventJsonText = jsonText;
        }
    }

    /// <summary>
    /// 包含序列化/反序列化方法
    /// </summary>
    public static class EventArgsConvert
    {
        /// <summary>
        /// 将事件数据类转化成字符串
        /// </summary>
        /// <param name="e">事件数据</param>
        /// <returns>转化后的字符串</returns>
        public static string SerializeObject(EventArgs e)
        {
            string Json = JsonConvert.SerializeObject(e);
            StandardTransportStreamEvent se = new StandardTransportStreamEvent(e.GetType().Name, Json);
            return JsonConvert.SerializeObject(se); //二次转换
        }

        /// <summary>
        /// 将字符串反转成事件数据
        /// </summary>
        /// <param name="text">字符串文本</param>
        /// <returns>事件数据</returns>
        public static EventArgs DeserializeObject(string text)
        {
            StandardTransportStreamEvent se = JsonConvert.DeserializeObject<StandardTransportStreamEvent>(text);
            //性能数据
            if(se.eventTypeName==typeof(PerformanceData).Name)
                return JsonConvert.DeserializeObject<PerformanceData>(se.eventJsonText);
            
            //通知本机已经启动
            if (se.eventTypeName == typeof(ClientStartNotificationEvent).Name)
                return JsonConvert.DeserializeObject<ClientStartNotificationEvent>(se.eventJsonText);

            //更改上传周期
            if (se.eventTypeName == typeof(ChangeUploadPeriodEvent).Name)
                return JsonConvert.DeserializeObject<ChangeUploadPeriodEvent>(se.eventJsonText);
            
            return new EventArgs(); //全部不匹配，返回空事件
        }
    }
}

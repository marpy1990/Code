/********************************************************************************
 * Author:  marpy
 * Date:    2013/9/29
 * Description: 模块处理器，实现模块功能。
 *              模块处理器之间并行运行，通过事件驱动和通信。
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorServer
{
    /// <summary>
    /// 模块处理器基类框架，提供完整的处理器通信功能，模块具体功能可由其派生类实现
    /// </summary>
    public class Processor
    {
        #region 处理器通信模块
        /// <summary>
        /// 绑定sender与EventArgs
        /// </summary>
        private struct EventBind
        {
            /// <summary>
            /// 事件发送者
            /// </summary>
            public Processor sender;

            /// <summary>
            /// 事件数据
            /// </summary>
            public EventArgs e;
        }

        /// <summary>
        /// 事件接收槽，暂存接收事件
        /// </summary>
        private List<EventBind> eventReceiveBuffer = new List<EventBind>();

        /// <summary>
        /// 控制对事件接收槽的修改，确保每次最多只有一个线程修改事件接收槽
        /// </summary>
        private Semaphore eventReceiveBufferSem = new Semaphore(1, 1);

        /// <summary>
        /// 发送事件给某一处理器
        /// </summary>
        /// <param name="receiver">接收处理器</param>
        /// <param name="e">事件数据</param>
        public void SendEvent(Processor receiver, EventArgs e)
        {
            EventBind bind = new EventBind();
            bind.sender = this;
            bind.e = e;
            receiver.eventReceiveBufferSem.WaitOne();    //准备修改事件缓冲区
            receiver.eventReceiveBuffer.Add(bind);
            receiver.eventReceiveBufferSem.Release();    //释放信号量
        }
        #endregion

        #region 事件驱动模块
        /// <summary>
        /// 声明处理事件方法的委托
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件数据</param>
        public delegate void DelegateEventHandle(Processor sender, EventArgs e);

        /// <summary>
        /// 事件处理委托表，建立事件数据与处理方法间的映射
        /// </summary>
        public Dictionary<Type, DelegateEventHandle> eventHandleTable = new Dictionary<Type, DelegateEventHandle>();

        /// <summary>
        /// 事件处理的时间周期，初始为infinite
        /// </summary>
        private TimeSpan _eventHandlePeriod = Timeout.InfiniteTimeSpan;

        /// <summary>
        /// 获取或设置一个值，该值为触发事件处理的时间周期
        /// </summary>
        public TimeSpan eventHandlePeriod
        {
            get
            {
                return this._eventHandlePeriod;
            }
            set
            {
                this._eventHandlePeriod = value;
                if (this._isStarted)     //启动
                {
                    this.eventHandleTrigger.Change(value, value);
                }
            }
        }

        /// <summary>
        /// 定时启动事件处理
        /// </summary>
        private Timer eventHandleTrigger = null;

        /// <summary>
        /// 事件处理程序
        /// </summary>
        private void EventHandle()
        {
            List<EventBind> eventHandleBuffer = new List<EventBind>();  //事件处理槽

            this.eventReceiveBufferSem.WaitOne();               //申请修改事件接收槽
            eventHandleBuffer = this.eventReceiveBuffer;
            this.eventReceiveBuffer = new List<EventBind>();    //清空事件接收槽
            this.eventReceiveBufferSem.Release();               //释放事件接收槽

            //if (eventHandleTable == null) return;
            //EditDate: 2013/10/2
            //Editor:   marpy
            //Reason:   eventHandleTable已赋初值
            foreach (EventBind bind in eventHandleBuffer)
            {
                Type type = bind.e.GetType();
                if (!PretreatToSkip(bind.e) && eventHandleTable.ContainsKey(type))
                {

                    Thread thread = new Thread(() => EventHandleThread(type, bind));    //新建子线程进行事件处理
                    thread.Start();
                }
            }
        }

        /// <summary>
        /// 预处理事件，用于减少新建的线程数量，可选
        /// </summary>
        /// <param name="type">事件数据</param>
        /// <returns>返回是否跳过事件处理</returns>
        protected virtual bool PretreatToSkip(EventArgs e)
        {
            return false;
        }

        /// <summary>
        /// 事件处理，同时注册或注销对应线程表
        /// </summary>
        /// <param name="type">事件类型</param>
        /// <param name="bind">事件参数包</param>
        private void EventHandleThread(Type type, EventBind bind)
        {
            DelegateEventHandle handler = eventHandleTable[type];
            Thread eventHandleThread = new Thread(() => handler(bind.sender, bind.e));  //采用lambda表达式传入事件处理方法的参数
            RegisterThread(eventHandleThread, handler);     //在线程表中注册该线程
            try
            {
                eventHandleThread.Start();  //尽量减少把错误报告给用户层
            }
            catch
            {
                ErrorHandle();
            }
            eventHandleThread.Join();                       //挂起等待
            CancelThread(eventHandleThread);                //处理结束，注销对应线程
        }

        /// <summary>
        /// 错误处理函数，可选
        /// </summary>
        protected virtual void ErrorHandle()
        {
            //可选
            throw (new Exception());
        }
        #endregion

        #region 线程管理模块
        /// <summary>
        /// 提供线程信息
        /// </summary>
        protected struct ThreadInformation
        {
            /// <summary>
            /// 线程开始时间
            /// </summary>
            public DateTime startTime;

            /// <summary>
            /// 产生线程的处理器
            /// </summary>
            public Processor creator;

            /// <summary>
            /// 用于处理线程的方法
            /// </summary>
            public DelegateEventHandle handler;
        }

        /// <summary>
        /// 正在运行的由Processor产生的线程表
        /// </summary>
        protected static Dictionary<Thread, ThreadInformation> runningThreadTable = new Dictionary<Thread, ThreadInformation>();

        /// <summary>
        /// 线程表信号量，确保每次最多只有一个线程访问线程表
        /// </summary>
        protected static Semaphore runningThreadTableSem = new Semaphore(1, 1);

        /// <summary>
        /// 注册线程至线程表
        /// </summary>
        /// <param name="thread">待注册线程</param>
        private void RegisterThread(Thread thread, DelegateEventHandle handler)
        {
            ThreadInformation threadInfo = new ThreadInformation();
            DateTime startTime = DateTime.Now;  //获取系统当前时间戳
            threadInfo.creator = this;
            threadInfo.startTime = startTime;
            threadInfo.handler = handler;
            runningThreadTableSem.WaitOne();    //准备修改线程表
            runningThreadTable.Add(thread, threadInfo);
            runningThreadTableSem.Release();    //释放信号量
        }

        /// <summary>
        /// 从线程表上注销对应线程
        /// </summary>
        /// <param name="thread">待注销线程</param>
        private void CancelThread(Thread thread)
        {
            runningThreadTableSem.WaitOne();    //准备修改线程表
            if (runningThreadTable.ContainsKey(thread))
            {
                runningThreadTable.Remove(thread);
            }
            runningThreadTableSem.Release();    //释放信号量
        }

        /// <summary>
        /// 注册的线程数
        /// </summary>
        public static int threadCount
        {
            get
            {
                return runningThreadTable.Count;
            }
        }

        /// <summary>
        /// 强制停止线程表上所有线程
        /// </summary>
        public static void ClearThreads()
        {
            runningThreadTableSem.WaitOne();
            foreach (Thread thread in runningThreadTable.Keys)
            {
                thread.Abort();
            }
            runningThreadTableSem.Release();
        }
        #endregion

        #region 处理器控制模块
        /// <summary>
        /// 处理器是否已经启动
        /// </summary>
        private bool _isStarted = false;

        /// <summary>
        /// 获取一个值，该值指示处理器是否已经启动
        /// </summary>
        public bool isStarted
        {
            get
            {
                return this._isStarted;
            }
        }

        /// <summary>
        /// 启动处理器
        /// </summary>
        public void Start()
        {
            if (_isStarted) return; //防止重复启动
            _isStarted = true;
            this.StartHandle();     //预加载方法
            processorJoinSem.WaitOne();     //阻塞Join方法
            this.eventHandleTrigger = new Timer((object state) => this.EventHandle());  //绑定定时时间处理方法
            this.eventHandleTrigger.Change(new TimeSpan(0), this._eventHandlePeriod);
        }

        /// <summary>
        /// 启动时的预加载方法
        /// </summary>
        protected virtual void StartHandle()
        {
            //用于继承
        }

        /// <summary>
        /// 停止处理器的事件驱动
        /// </summary>
        private void StopEventHandle()
        {
            this.eventHandleTrigger.Dispose();
        }

        /// <summary>
        /// 用于同步处理器停止方法的信号量
        /// </summary>
        private Semaphore stopFuncSynSem;

        /// <summary>
        /// 等待处理器所有子线程释放后停止处理器
        /// </summary>
        /// <param name="timeout">最大等待时间</param>
        /// <returns>子线程是否在规定时间内全部释放</returns>
        public bool TryToStop(TimeSpan timeout)
        {
            if (!this._isStarted) return true;
            PrepareToStop();    //执行stop之前的准备工作
            this.StopEventHandle();
            bool isStoped = false;
            Thread thread = new Thread(() => this.WaitForAllThreadStop(out isStoped));
            stopFuncSynSem = new Semaphore(0, 1);
            thread.Start();
            stopFuncSynSem.WaitOne(timeout);   //父线程挂起
            if (isStoped)
            {
                this._isStarted = false;    //置_isStarted为假
                processorJoinSem.Release(); //唤醒Join方法
            }
            return isStoped;
        }

        /// <summary>
        /// 在执行tryToStop之前的准备工作
        /// </summary>
        protected virtual void PrepareToStop()
        {
            //用于继承
        }

        /// <summary>
        /// Join方法阻塞处理器信号量，在处理器停止时释放
        /// </summary>
        private Semaphore processorJoinSem = new Semaphore(1, 1);

        /// <summary>
        /// 挂起并等待所有子线程结束
        /// </summary>
        private void WaitForAllThreadStop(out bool isStoped)
        {
            List<Thread> waitingThreads = new List<Thread>(0);
            runningThreadTableSem.WaitOne();    //申请访问子线程
            foreach (Thread thread in runningThreadTable.Keys)
            {
                if (runningThreadTable[thread].creator == this)     //线程是由自己产生的
                {
                    waitingThreads.Add(thread);
                }
            }
            runningThreadTableSem.Release();    //释放访问权

            //获取线程信息后再等待，防止由“占有并申请资源”而产生的死锁
            foreach (Thread thread in waitingThreads)
            {
                thread.Join();
            }
            isStoped = true;
            stopFuncSynSem.Release();   //唤醒TryToStop线程
        }

        /// <summary>
        /// 强制停止处理器，该方法仅仅唤醒被Join阻塞的父线程，而且不执行PrepareToStop方法，不推荐
        /// </summary>
        public void Stop()
        {
            this.StopEventHandle();
            this._isStarted = false;
            processorJoinSem.Release();
        }

        /// <summary>
        /// 挂起等待直到处理器停止
        /// </summary>
        public void Join()
        {
            if (!_isStarted) return;
            processorJoinSem.WaitOne();
        }
        #endregion
    }
}

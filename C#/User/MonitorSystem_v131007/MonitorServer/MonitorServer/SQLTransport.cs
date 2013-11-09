using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorEvent;
using System.IO;
using System.Data.SqlClient;

namespace MonitorServer
{
    class SQLTransport:Processor
    {
        private SqlConnection sqlCon = new SqlConnection();

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PerformanceDataSave(Processor sender, EventArgs e)
        {
            PerformanceData pd = (PerformanceData)e;

            try
            {
                sqlCon.Open();
                string sql = SQLQuery(pd);
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                cmd.ExecuteNonQuery();
                sqlCon.Close();
            }
            catch
            {
                SendEvent(Global.sqlTrans, e);
            }
        }

        #region SQL语句
        const string creatTable =
@"if object_id(N'监控性能数据',N'U') is null
begin
CREATE TABLE 监控性能数据
(
机器名 varchar(50) ,
项目名 varchar(50) ,
实例名 varchar(50) ,
日期   varchar(20) ,
性能值 varchar(20) ,
primary key(机器名,项目名,实例名,日期)
)
end";

        private string SQLQuery(PerformanceData pd)
        {
            string machineName=pd.machineName;
            string categoryName=pd.categoryName;
            DateTime date=pd.date;

            string sqlStr="";
            foreach(string instanceName in pd.instanceTable.Keys)
            {
                string value = pd.instanceTable[instanceName].ToString();
                if (value.Length > 20)
                    value = value.Substring(0, 20);
                sqlStr += @"insert 监控性能数据(机器名,项目名,实例名,日期,性能值) values("
                    + "'" + pd.machineName + "',"
                    + "'" + pd.categoryName + "',"
                    + "'" + instanceName + "',"
                    + "'" + pd.date.ToString("yyyy-MM-dd-HH-mm-ss") + "',"
                    + "'" + value + "')  ";
            }
            return sqlStr;
        }

        #endregion

        /// <summary>
        /// 载入数据库连接指令并查询表是否存在
        /// </summary>
        protected override void StartHandle()
        {
            FileStream fs = new FileStream(Global.sqlConnectFile, FileMode.Open);
            StreamReader m_streamReader = new StreamReader(fs);
            string connectionString = m_streamReader.ReadToEnd();
            m_streamReader.Close();
            fs.Close();
            sqlCon.ConnectionString = connectionString;
            sqlCon.Open();
            SqlCommand cmd = new SqlCommand(creatTable, sqlCon);
            cmd.ExecuteNonQuery();
            sqlCon.Close();
        }

        public SQLTransport()
        {
            eventHandlePeriod = new TimeSpan(0, 0, 0, 0, 100);
            eventHandleTable.Add(typeof(PerformanceData), this.PerformanceDataSave);
        }
    }
}

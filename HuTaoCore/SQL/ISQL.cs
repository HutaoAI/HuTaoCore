using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuTaoCore.SQL
{
    public interface ISQL
    {
        /// <summary>
        /// 增删改
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        int ExecuteNonQuery(string sql, params SqlParameter[] ps);

        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        object ExecuteScalar(string sql, params SqlParameter[] ps);

        SqlDataReader ExecuteReader(string sql, params SqlParameter[] ps);

        DataSet GetDataSet(string sql, params SqlParameter[] ps);
       
    }
}

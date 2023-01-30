using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuTaoCore.SQL
{
    public class SqlServerHelper:ISQL
    {
        public SqlServerHelper(string consql)
        {
            SqlConn = consql;
        }

        private string SqlConn;

        /// <summary>
        /// 执行增删改语句,返回受影响行数,否者返回-1
        /// </summary>
        /// <param name="sql">执行sql语句</param>
        /// <param name="ps">参数化查询</param>
        /// <returns>返回受影响行数</returns>
        public int ExecuteNonQuery(string sql, params SqlParameter[] ps)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(SqlConn))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddRange(ps);
                        conn.Open();
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        /// <summary>
        /// 执行查询语句,成功返回 1
        /// </summary>
        /// <param name="sql">执行查询的sql语句</param>
        /// <param name="ps"></param>
        /// <returns>成功返回 1</returns>
        public object ExecuteScalar(string sql, params SqlParameter[] ps)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(SqlConn))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddRange(ps);
                        conn.Open();
                        return cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        /// <summary>
        /// 提供数据查询并返回操作
        /// </summary>
        /// <param name="sql">执行sql查询语句</param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public SqlDataReader ExecuteReader(string sql, params SqlParameter[] ps)
        {
            SqlConnection conn = new SqlConnection(SqlConn);
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddRange(ps);
                    conn.Open();
                    return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                }
            }
            catch (Exception e)
            {
                conn.Dispose();
                throw new Exception(e.ToString());
            }
        }
        /// <summary>
        /// 查询数据,返回查询到数据
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <param name="ps">参数化查询</param>
        /// <returns></returns>
        public DataSet GetDataSet(string sql, params SqlParameter[] ps)
        {
            DataSet ds = new DataSet();
            using (SqlDataAdapter sda = new SqlDataAdapter(sql, SqlConn))
            {
                sda.SelectCommand.Parameters.AddRange(ps);
                sda.Fill(ds);
            }
            return ds;
        }
        /// <summary>
        /// 判断连接成功数据库
        /// </summary>
        /// <returns></returns>
        public bool IsConncet()
        {
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(SqlConn))
                {
                    conn.Open();
                    conn.Close();
                    return true;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }
    }
}

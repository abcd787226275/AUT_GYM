using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Configuration;
using KP.Common;
using System.Data.SqlClient;

namespace KP.Common
{
    public class ClsDataBaseSQLMaker
    {

        static public ClsDataBaseSQL MakeDataBaseSQL()
        {
            HttpContext.Current.Session["LoginDB"] = "";

            string connStr = HttpContext.Current.Session["LoginDB"].ToString();

            if (connStr.Equals(""))
            {
                return null;
            }

            return new ClsDataBaseSQL(connStr);
        }

    }

    public class ClsDataBaseSQL : IDisposable
    {
        private SqlConnection con;

        private SqlDataAdapter da;

        private DataSet ds;

        private string connString;

        public static string DATABASEOWNER = "dbo.";

        public ClsDataBaseSQL()
        {
            string server = ConfigurationManager.AppSettings["Server"].ToString();
            string DbName = ConfigurationManager.AppSettings["DbName"].ToString();
            string PW = ConfigurationManager.AppSettings["PW"].ToString();
            this.connString = "server=" + server + ";uid=sa;pwd=" + PW + ";database=" + DbName + "";

            this.con = new SqlConnection(connString);
        }

        public ClsDataBaseSQL(string DB)
        {
            /*
            string[] array = DB.Split(new char[]
            {
                ','
            });
            this.connString = string.Concat(new string[]
            {
                "data source=",
                array[0],
                ";initial catalog=",
                array[2],
                ";persist security info=False;user id=sa;pwd=",
                array[1],
                "; workstation id=kp;packet size=4096;"
            });*/

        }

        public bool TestConnection(string Server, string saPass)
        {
            this.connString = string.Concat(new string[]
            {
                "data source=",
                Server,
                ";initial catalog=master;persist security info=False;user id=sa;pwd=",
                saPass,
                "; workstation id=kp;packet size=4096;"
            });
            this.con = new SqlConnection(this.connString);
            bool result;
            try
            {
                this.con.Open();
                result = true;
            }
            catch (Exception ex)
            {
                Error.Log(ex.Message);
                this.con.Close();
                result = false;
            }
            return result;
        }

        public static string GetDBFuncFullName(string pDBFuncName)
        {
            return ClsDataBaseSQL.DATABASEOWNER + pDBFuncName;
        }

        /*public DataSet ProcPages(string SqlString, string sID, string SortField, int CurrentPageIndex, int PageSize, out int RecPages)
        {
            SqlParameter[] array = new SqlParameter[]
            {
                this.MakeInParam("@pSQL", SqlDbType.VarChar, SqlString.Length, SqlString),
                this.MakeInParam("@pIDCol", SqlDbType.VarChar, 8000, sID),
                this.MakeInParam("@pOrder", SqlDbType.VarChar, 500, SortField),
                this.MakeInParam("@pCurPage", SqlDbType.Int, 4, CurrentPageIndex),
                this.MakeInParam("@pPageSize", SqlDbType.Int, 4, PageSize),
                this.MakeOutParam("@pRecNums", SqlDbType.Int, 4),
                this.MakeOutParam("@pRecPages", SqlDbType.Int, 4)
            };
            DataSet result = this.dsRunProc(ClsDataBaseSQL.GetDBFuncFullName("sp_CommQueryProc"), array);
            RecPages = ClsUtil.ToInt32(array[6].Value);
            return result;
        }*/

        public string SelectDataBaseFirst(string strSQL)
        {
            this.Open();
            SqlCommand sqlCommand = new SqlCommand(strSQL, this.con);
            string result;
            try
            {
                string text = sqlCommand.ExecuteScalar().ToString();
                this.Close();
                sqlCommand.Dispose();
                this.Dispose();
                result = text;
            }
            catch (Exception ex)
            {
                //Error.Log(ex.Message);
                this.Close();
                sqlCommand.Dispose();
                this.Dispose();
                result = null;
            }
            return result;
        }

        public DataSet SelectDataBase(string strSQL)
        {
            try
            {
                this.Open();
                this.ds = new DataSet();
                this.da = new SqlDataAdapter(strSQL, this.con);
                this.da.Fill(this.ds);
                this.Close();
            }
            catch (Exception ex)
            {
                Error.Log(ex.Message);
                this.Close();
            }
            return this.ds;
        }

        public int ExecDataBase(string strSQL)
        {
            this.Open();
            this.Open();
            int result = 0;
            this.da = new SqlDataAdapter(strSQL, this.con);

            this.Close();
            return result;
        }

        public int OperDataBase(string strSQL)
        {
            this.Open();
            SqlCommand sqlCommand = new SqlCommand(strSQL, this.con);
            int result;
            try
            {
                sqlCommand.ExecuteNonQuery();
                this.Close();
                sqlCommand.Dispose();
                this.Dispose();
                result = 1;
            }
            catch (Exception ex)
            {
                // Error.Log(ex.Message);
                this.Close();
                sqlCommand.Dispose();
                this.Dispose();
                result = -1;
            }
            return result;
        }

        /// <summary>
        /// 执行更新或插入操作，如果insertedID不为null，则同时返回刚插入的ID
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        private int UpdateOrInsertDataBase(string strSQL, ref string insertedID)
        {
            string sql = strSQL;

            if (insertedID != null)
            {
                sql += "; select @IdentityId=SCOPE_IDENTITY();";
            }

            this.Open();
            SqlCommand sqlCommand = new SqlCommand(sql, this.con);
            int result;
            try
            {
                sqlCommand.Transaction = this.con.BeginTransaction();
                // Error.Log(sql);

                if (insertedID != null)
                {
                    SqlParameter IdentityPara = new SqlParameter("@IdentityId", SqlDbType.Int);
                    IdentityPara.Direction = ParameterDirection.Output;

                    sqlCommand.Parameters.Add(IdentityPara);
                }

                int num = sqlCommand.ExecuteNonQuery();

                if (insertedID != null)
                {
                    insertedID = sqlCommand.Parameters["@IdentityId"].Value.ToString();
                }

                sqlCommand.Transaction.Commit();

                this.Close();
                sqlCommand.Dispose();
                this.Dispose();
                result = num;
            }
            catch (Exception ex)
            {
                // Error.Log(sql + "[" + ex.Message + "]");
                sqlCommand.Transaction.Rollback();
                this.Close();
                sqlCommand.Dispose();
                this.Dispose();
                result = -1;
            }
            return result;
        }

        public int InsertDataBase(string strSQL, out string insertedID)
        {
            string tmpId = "";

            int result = UpdateOrInsertDataBase(strSQL, ref tmpId);

            insertedID = tmpId;

            return result;
        }

        public int UpdateDataBase(string strSQL)
        {
            string para = null;

            return UpdateOrInsertDataBase(strSQL, ref para);
        }

        public int InsertData(string mSQL, DataSet[] dDsArray, string[] dTableArray, string[] dIDArray, string dAddUpdateSQL = "")
        {
            if (dDsArray.Length != dTableArray.Length ||
                dDsArray.Length != dIDArray.Length)
            {
                return -1;
            }


            int detailNum = dDsArray.Length;

            DataTable[] dTableHeaderArray = new DataTable[detailNum];
            for (int loop = 0; loop < detailNum; loop++)
            {
                dTableHeaderArray[loop] = SelectDataBase("select * from " + dTableArray[loop] + " where 1=2").Tables[0];
            }

            this.Open();
            SqlCommand sqlCommand = new SqlCommand();
            int result;
            try
            {
                sqlCommand.Connection = this.con;
                sqlCommand.CommandText = mSQL;
                //Error.Log(mSQL);
                sqlCommand.Transaction = this.con.BeginTransaction();
                int num = sqlCommand.ExecuteNonQuery();

                for (int loop = 0; loop < detailNum; loop++)
                {
                    DataSet dDs = dDsArray[loop];
                    string dTable = dTableArray[loop];
                    string dID = dIDArray[loop];

                    //获取明细表的表头，目的是在插入的时候，排除dDS有，但dTable中没有的列
                    DataTable dTableHeader = dTableHeaderArray[loop];

                    int count = dDs.Tables[0].Columns.Count;
                    string text = "";
                    string text2 = "";
                    DataView dataView = new DataView(dDs.Tables[0]);
                    for (int i = 0; i < dataView.Count; i++)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            if (dDs.Tables[0].Columns[j].ColumnName.ToLower() != dID.ToLower() && dTableHeader.Columns.Contains(dDs.Tables[0].Columns[j].ColumnName))
                            {
                                if (dDs.Tables[0].Columns[j].DataType.IsValueType && dDs.Tables[0].Columns[j].DataType != typeof(DateTime))
                                {
                                    if (dataView[i][dDs.Tables[0].Columns[j].ColumnName].ToString() != "")
                                    {
                                        text = text + ",[" + dDs.Tables[0].Columns[j].ColumnName + "]";
                                        text2 = text2 + "," + dataView[i][dDs.Tables[0].Columns[j].ColumnName].ToString();
                                    }

                                }
                                else
                                {
                                    text = text + ",[" + dDs.Tables[0].Columns[j].ColumnName + "]";
                                    text2 = text2 + ",'" + dataView[i][dDs.Tables[0].Columns[j].ColumnName].ToString() + "'";
                                }
                            }
                        }
                        text = text.Remove(0, 1);
                        text2 = text2.Remove(0, 1);
                        sqlCommand.CommandText = string.Concat(new string[]
                        {
                        "insert into ",
                        dTable,
                        "(",
                        text,
                        ")values(",
                        text2,
                        ")"
                        });
                        //  Error.Log(sqlCommand.CommandText);
                        text = "";
                        text2 = "";
                        num = sqlCommand.ExecuteNonQuery();
                    }
                }

                //附加更新
                if (dAddUpdateSQL != "")
                {
                    sqlCommand.CommandText = string.Concat(new string[]           {
                        dAddUpdateSQL
                    });
                    ///Error.Log(sqlCommand.CommandText);
                    num = sqlCommand.ExecuteNonQuery();
                    if (num == 0)
                    {
                        throw new Exception("更新失败!");
                    }
                }
                sqlCommand.Transaction.Commit();
                this.Close();
                sqlCommand.Dispose();
                this.Dispose();
                result = num;
            }
            catch (Exception ex)
            {
                // Error.Log(ex.Message);
                sqlCommand.Transaction.Rollback();
                this.Close();
                sqlCommand.Dispose();
                this.Dispose();
                result = -1;
            }
            return result;
        }

        public int InsertData(string mSQL, DataSet dDs, string dTable, string dID, string dAddUpdateSQL = "")
        {
            return InsertData(mSQL, new DataSet[] { dDs }, new string[] { dTable }, new string[] { dID }, dAddUpdateSQL);
        }

        /// <summary>
        /// 构造删除命令
        /// </summary>
        /// <param name="sheetIdName"></param>
        /// <param name="detailDs"></param>
        /// <param name="detailTableName"></param>
        /// <param name="detailIdName"></param>
        /// <returns></returns>
        protected string ConstructDeleteDetailSqlStr(string sheetIdName, DataSet detailDs, string detailTableName)
        {
            //删除原有的明细
            if (!detailDs.Tables[0].Columns.Contains(sheetIdName))
            {
                return "";
            }

            if (detailDs.Tables[0].Rows.Count <= 0)
            {
                return "";
            }

            string sheetid = detailDs.Tables[0].Rows[0][sheetIdName].ToString();

            return "delete from " + detailTableName + " where " + sheetIdName + " = " + sheetid;
        }


        /// <summary>
        /// 更新主表和明细表，采用先删除所有旧明细，再插入新明细的方式（覆盖式更新)
        /// </summary>
        /// <param name="masterTableSQL">更新主表信息的SQL语句</param>
        /// <param name="sheetIdName">明细表中包含主表关键字的字段名（例如SheetID)</param>
        /// <param name="detailDs">明细数据集</param>
        /// <param name="detailTableName">明细表名称</param>
        /// <param name="detailIdName">明细表的关键字列名</param>
        /// <returns></returns>
        public int UpdateData(string masterTableSQL,
            string[] sheetIdNameArray, DataSet[] detailDsArray, string[] detailTableNameArray, string[] detailIdNameArray, string addUpdateSQL = "")
        {
            if (sheetIdNameArray.Length != detailDsArray.Length
                ||
                sheetIdNameArray.Length != detailTableNameArray.Length
                ||
                sheetIdNameArray.Length != detailIdNameArray.Length
                )
            {
                return -1;
            }

            int detailNum = sheetIdNameArray.Length;

            DataTable[] dTableHeaderArray = new DataTable[detailNum];
            for (int loop = 0; loop < detailNum; loop++)
            {
                dTableHeaderArray[loop] = SelectDataBase("select * from " + detailTableNameArray[loop] + " where 1=2").Tables[0];
            }

            this.Open();
            SqlCommand sqlCommand = new SqlCommand();
            int result = 0;
            try
            {
                sqlCommand.Connection = this.con;

                /*步骤一：修改主表记录*/
                sqlCommand.CommandText = masterTableSQL;
                //Error.Log(masterTableSQL);
                sqlCommand.Transaction = this.con.BeginTransaction();

                int num = sqlCommand.ExecuteNonQuery();

                for (int arrayIndex = 0; arrayIndex < detailNum; arrayIndex++)
                {
                    DataSet detailDs = detailDsArray[arrayIndex];
                    string detailTableName = detailTableNameArray[arrayIndex];
                    string detailIdName = detailIdNameArray[arrayIndex];
                    string sheetIdName = sheetIdNameArray[arrayIndex];

                    //获取明细表的表头，目的是在插入的时候，排除dDS有，但dTable中没有的列
                    DataTable dTableHeader = dTableHeaderArray[arrayIndex];

                    /*步骤二：删除原有的明细*/
                    sqlCommand.CommandText = ConstructDeleteDetailSqlStr(sheetIdName, detailDs, detailTableName);
                    // Error.Log(sqlCommand.CommandText);

                    if (!string.IsNullOrEmpty(sqlCommand.CommandText))
                    {
                        sqlCommand.ExecuteNonQuery();
                    }

                    /*步骤三：插入新的明细*/
                    int count = detailDs.Tables[0].Columns.Count;
                    string text = "";
                    string text2 = "";
                    DataView dataView = new DataView(detailDs.Tables[0]);
                    for (int i = 0; i < dataView.Count; i++)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            if (detailDs.Tables[0].Columns[j].ColumnName.ToLower() != detailIdName.ToLower() && dTableHeader.Columns.Contains(detailDs.Tables[0].Columns[j].ColumnName))
                            {
                                if (detailDs.Tables[0].Columns[j].DataType.IsValueType && detailDs.Tables[0].Columns[j].DataType != typeof(DateTime))
                                {
                                    if (!string.IsNullOrEmpty(dataView[i][detailDs.Tables[0].Columns[j].ColumnName].ToString()))
                                    {
                                        text = text + ",[" + detailDs.Tables[0].Columns[j].ColumnName + "]";
                                        text2 = text2 + "," + dataView[i][detailDs.Tables[0].Columns[j].ColumnName].ToString();
                                    }
                                }
                                else
                                {
                                    text = text + ",[" + detailDs.Tables[0].Columns[j].ColumnName + "]";
                                    text2 = text2 + ",'" + dataView[i][detailDs.Tables[0].Columns[j].ColumnName].ToString() + "'";
                                }
                            }
                        }
                        text = text.Remove(0, 1);
                        text2 = text2.Remove(0, 1);
                        sqlCommand.CommandText = string.Concat(new string[]
                        {
                        "insert into ",
                        detailTableName,
                        "(",
                        text,
                        ")values(",
                        text2,
                        ")"
                        });
                        // Error.Log(sqlCommand.CommandText);
                        text = "";
                        text2 = "";
                        num = sqlCommand.ExecuteNonQuery();
                    }
                }

                //更新
                if (addUpdateSQL != "")
                {
                    sqlCommand.CommandText = string.Concat(new string[] { addUpdateSQL });
                    //  Error.Log(sqlCommand.CommandText);
                    num = sqlCommand.ExecuteNonQuery();
                    if (num == 0)
                    {
                        throw new Exception("更新失败!");
                    }
                }

                sqlCommand.Transaction.Commit();
                this.Close();
                sqlCommand.Dispose();
                this.Dispose();
                result = num;

            }

            catch (Exception ex)
            {
                // Error.Log(ex.Message);
                sqlCommand.Transaction.Rollback();
                this.Close();
                sqlCommand.Dispose();
                this.Dispose();
                result = -1;
            }
            return result;

        }

        /// <summary>
        /// 更新主表和明细表，采用先删除所有旧明细，再插入新明细的方式（覆盖式更新)
        /// </summary>
        /// <param name="masterTableSQL">更新主表信息的SQL语句</param>
        /// <param name="sheetIdName">明细表中包含主表关键字的字段名（例如SheetID)</param>
        /// <param name="detailDs">明细数据集</param>
        /// <param name="detailTableName">明细表名称</param>
        /// <param name="detailIdName">明细表的关键字列名</param>
        /// <returns></returns>
        public int UpdateData(string masterTableSQL, string sheetIdName, DataSet detailDs, string detailTableName, string detailIdName, string addUpdateSQL = "")
        {
            return UpdateData(masterTableSQL,
                new string[] { sheetIdName }, new DataSet[] { detailDs },
                new string[] { detailTableName }, new string[] { detailIdName },
                addUpdateSQL);
        }


        /// 更新主表和明细表,只更新不删除
        /// </summary>
        /// <param name="mSQL">更新主表信息的SQL语句</param>
        /// <param name="dDs">明细表中包含主表关键字的字段名（例如SheetID)</param>
        /// <param name="dTable">明细数据集</param>
        /// <param name="dID">明细ID</param>
        /// <returns></returns>
        public int UpdateData(string mSQL, DataSet dDs, string dTable, string dID)
        {
            //获取明细表的表头，目的是在更新的时候，排除dDS有，但dTable中没有的列
            DataTable dTableHeader = SelectDataBase("select * from " + dTable + " where 1=2").Tables[0];

            this.Open();
            SqlCommand sqlCommand = new SqlCommand();
            int result;
            try
            {
                sqlCommand.Connection = this.con;
                sqlCommand.CommandText = mSQL;
                //  Error.Log(mSQL);
                sqlCommand.Transaction = this.con.BeginTransaction();

                int num = sqlCommand.ExecuteNonQuery();


                int count = dDs.Tables[0].Columns.Count;
                string text = "";
                DataView dataView = new DataView(dDs.Tables[0]);
                for (int i = 0; i < dataView.Count; i++)
                {
                    for (int j = 0; j < count; j++)
                    {
                        if (dDs.Tables[0].Columns[j].ColumnName.ToLower() != dID.ToLower() && dTableHeader.Columns.Contains(dDs.Tables[0].Columns[j].ColumnName))
                        {
                            if (dDs.Tables[0].Columns[j].DataType.IsValueType)
                            {
                                if (dataView[i][dDs.Tables[0].Columns[j].ColumnName].ToString() != "")
                                {
                                    text = string.Concat(new string[]
                                    {
                                    text,
                                    ",",
                                    dDs.Tables[0].Columns[j].ColumnName,
                                    " = ",
                                    dataView[i][dDs.Tables[0].Columns[j].ColumnName].ToString()
                                    });
                                }
                            }
                            else
                            {
                                text = string.Concat(new string[]
                                {
                                    text,
                                    ",",
                                    dDs.Tables[0].Columns[j].ColumnName,
                                    " = '",
                                    dataView[i][dDs.Tables[0].Columns[j].ColumnName].ToString(),
                                    "'"
                                });
                            }
                        }
                    }
                    text = text.Remove(0, 1);
                    sqlCommand.CommandText = string.Concat(new string[]
                    {
                        "update ",
                        dTable,
                        " set ",
                        text,
                        " where ",
                        dID,
                        "=",
                        dataView[i][dID].ToString()
                    });
                    //  Error.Log(sqlCommand.CommandText);
                    text = "";
                    num = sqlCommand.ExecuteNonQuery();
                }


                sqlCommand.Transaction.Commit();
                this.Close();
                sqlCommand.Dispose();
                this.Dispose();
                result = num;
            }

            catch (Exception ex)
            {
                // Error.Log(ex.Message);
                sqlCommand.Transaction.Rollback();
                this.Close();
                sqlCommand.Dispose();
                this.Dispose();
                result = -1;
            }
            return result;
        }

        public int RunProc(string procName)
        {
            SqlCommand sqlCommand = this.CreateCommand(procName, null);
            sqlCommand.CommandTimeout = 60;
            sqlCommand.ExecuteNonQuery();
            this.Close();
            return (int)sqlCommand.Parameters["ReturnValue"].Value;
        }

        public int RunProc(string procName, SqlParameter[] prams)
        {
            SqlCommand sqlCommand = this.CreateCommand(procName, prams);
            sqlCommand.CommandTimeout = 60;
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Error.Log(ex.Message);
            }

            this.Close();
            return (int)sqlCommand.Parameters["ReturnValue"].Value;
        }

        public void RunProc(string procName, out SqlDataReader dataReader)
        {
            SqlCommand sqlCommand = this.CreateCommand(procName, null);
            sqlCommand.CommandTimeout = 60;
            dataReader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public void RunProc(string procName, SqlParameter[] prams, out SqlDataReader dataReader)
        {
            SqlCommand sqlCommand = this.CreateCommand(procName, prams);
            sqlCommand.CommandTimeout = 60;
            dataReader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public DataSet dsRunProc(string procName, SqlParameter[] prams)
        {
            this.ds = new DataSet();
            SqlCommand selectCommand = this.CreateCommand(procName, prams);
            selectCommand.CommandTimeout = 60;
            this.da = new SqlDataAdapter(selectCommand);
            this.da.Fill(this.ds);
            return this.ds;
        }

        private SqlCommand CreateCommand(string procName, SqlParameter[] prams)
        {
            this.Open();
            SqlCommand sqlCommand = new SqlCommand(procName, this.con);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            if (prams != null)
            {
                for (int i = 0; i < prams.Length; i++)
                {
                    SqlParameter value = prams[i];
                    sqlCommand.Parameters.Add(value);
                }
            }
            sqlCommand.Parameters.Add(new SqlParameter("ReturnValue", SqlDbType.Int, 4, ParameterDirection.ReturnValue, false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return sqlCommand;
        }

        private void Open()
        {
            try
            {
                if (this.con == null)
                {
                    this.con = new SqlConnection(this.connString);
                }
                if (this.con.State == ConnectionState.Closed)
                {
                    this.con.Open();
                }
            }
            catch (Exception ex)
            {
                //  Error.Log(ex.Message);
            }
        }

        public void Close()
        {
            if (this.con != null)
            {
                this.con.Close();
            }
        }

        public void Dispose()
        {
            if (this.con != null)
            {
                this.con.Dispose();
                this.con = null;
            }
        }

        public SqlParameter MakeInParam(string ParamName, SqlDbType DbType, int Size, object Value)
        {
            return this.MakeParam(ParamName, DbType, Size, ParameterDirection.Input, Value);
        }

        public SqlParameter MakeOutParam(string ParamName, SqlDbType DbType, int Size)
        {
            return this.MakeParam(ParamName, DbType, Size, ParameterDirection.Output, null);
        }

        public SqlParameter MakeReturnParam(string ParamName, SqlDbType DbType, int Size)
        {
            return this.MakeParam(ParamName, DbType, Size, ParameterDirection.ReturnValue, null);
        }

        public SqlParameter MakeParam(string ParamName, SqlDbType DbType, int Size, ParameterDirection Direction, object Value)
        {
            SqlParameter sqlParameter;
            if (Size > 0)
            {
                sqlParameter = new SqlParameter(ParamName, DbType, Size);
            }
            else
            {
                sqlParameter = new SqlParameter(ParamName, DbType);
            }
            sqlParameter.Direction = Direction;
            if (Direction != ParameterDirection.Output || Value != null)
            {
                sqlParameter.Value = Value;
            }
            return sqlParameter;
        }

        public static string DbClassAbout()
        {
            return "功能：通用的数据库处理类，作者：孔勇，邮箱：kypine@163.com，日期：2006年10月25日。";
        }


        /// <summary>
        /// 将表插入到数据库
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="connectString"></param>
        /// <returns></returns>
        public bool DataTableToSQLServer(DataTable dt)
        {
            string connectionString = connString;

            DataTable cpy = dt.Copy();

            //去除数据库中没有的列
            string sql = "select * from " + dt.TableName;
            DataTable tableInDbs = SelectDataBase(sql).Tables[0];

            foreach (DataColumn col in dt.Columns)
            {
                if (!tableInDbs.Columns.Contains(col.ColumnName))
                {
                    cpy.Columns.Remove(col.ColumnName);
                }
            }

            using (SqlConnection destinationConnection = new SqlConnection(connectionString))
            {
                destinationConnection.Open();

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(destinationConnection))
                {
                    try
                    {
                        bulkCopy.DestinationTableName = cpy.TableName;
                        bulkCopy.BatchSize = cpy.Rows.Count;

                        bulkCopy.WriteToServer(cpy);
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                    finally
                    {

                    }
                }
            }

            return true;

        }

    }
}
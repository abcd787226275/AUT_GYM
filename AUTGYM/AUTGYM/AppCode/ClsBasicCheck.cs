using KP.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;


namespace KP.Common
{
    public class ClsBasicCheck
    {
        private ClsDataBaseSQL dbs;

        public ClsBasicCheck()
        {
            dbs = new ClsDataBaseSQL();
            string DbName = ConfigurationManager.AppSettings["DbName"].ToString();
        }

        public string GetUserID()
        {
            string UserID;
            string sql = " select UserID from Users where FirstName='Sam'";
          
                DataView dv = new DataView(dbs.SelectDataBase(sql).Tables[0]);
                DataTable dt = dv.Table;
                UserID = Convert.ToString(dt.Rows[0]["UserID"]);
            
        

            return UserID;
        }
    }
}
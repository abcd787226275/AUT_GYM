using KP.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using static AUTGYM.Models.App;

namespace KP.Common
{
    public class ClsCourse
    {
        private ClsDataBaseSQL dbs;

        public ClsCourse()
        {
            dbs = new ClsDataBaseSQL();
            string DbName = ConfigurationManager.AppSettings["DbName"].ToString();
        }

        //按条件查询课程
        public object ShowCourse(string check)
        {
            List<TimeTable> list1 = new List<TimeTable>();

            string sql = @"  select CourseName,[Date],MemberJoint,[Status] from TimeTable 
  where CourseName like '%" + check + @"%'
  or convert(varchar(1024),[Date],120) like '%" + check + @"%'";
            try
            {
                DataView dv = new DataView(dbs.SelectDataBase(sql).Tables[0]);
                DataTable dt = dv.Table;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    list1.Add(new TimeTable
                    {

                        CourseName = Convert.ToString(dt.Rows[i]["CourseName"]),
                        Date = Convert.ToString(dt.Rows[i]["Date"]),
                        MemberJoint = Convert.ToString(dt.Rows[i]["MemberJoint"]),
                        Status = Convert.ToString(dt.Rows[i]["Status"]),
                        
                });
            }
            }
            catch (Exception e)
            {
                return "false";
            }
            return list1;
        }

        //选择课程
        public int ChooseCourse(string CourseName,string CardNumber,string BookID)
        {
            int check = 0;
            string sql = @"  declare @CourseName nvarchar(1024), @CardNumber nvarchar(1024),@BookID int,@result int 
  exec [dbo].[ChooseCourse] '"+CourseName+@"','"+CardNumber+@"','"+BookID+@"',@result output
  select @result as a";

            try
            {
                DataView dv = new DataView(dbs.SelectDataBase(sql).Tables[0]);
                DataTable dt = dv.Table;
                check = Convert.ToInt32(dt.Rows[0]["a"]);
            }
            catch (Exception e)
            {
                check = -2; //后台出错
            }

            return check;
        }
    }
}
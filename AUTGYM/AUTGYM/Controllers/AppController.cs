using KP.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace AUTGYM.Controllers
{
    public class AppController : ApiController
    {
        [HttpGet]
        public object CheckMemberID()
        {
            ClsBasicCheck bc = new ClsBasicCheck();
            string UserID = bc.GetUserID();

            return UserID;
        }

        [HttpGet]
        public object Hello()
        {
            string hello = "Hello!";
            return hello;
        }

        /// <summary>
        /// Sam Chen
        /// 2018/05/21
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        [HttpGet]
        //查询课程
        public object ShowCourse(string check)
        {
            ClsCourse co = new ClsCourse();
            var result = co.ShowCourse(check);

            //后台出错
            if (result.ToString()=="false")
            {
                result = "Something wrong with the App, Please communicate with AUT GYM as soon as possible!";
                return result.ToString();
            }

            return result;
        }


        //选择课程
        [HttpGet]
        public object ChooseCourse(string CourseName,string CardNumber,string BookID)
        {
            int check = -1;
            string result = null;
            ClsCourse co = new ClsCourse();
            check = co.ChooseCourse(CourseName,CardNumber,BookID);

            if (check==1)
            {
                result = "Your Booking for "+CourseName+" Success!";
            }else if (check==0)
            {
                result = "Oops, This class has already full.Please choose another one.";
            }else if (check==-1||check==-2)
            {
                result = "Something wrong with the App, Please communicate with AUT GYM as soon as possible!";
            }

            return result;
        }
        //
    }
}
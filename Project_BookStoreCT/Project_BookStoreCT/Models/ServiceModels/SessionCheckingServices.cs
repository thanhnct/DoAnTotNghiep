using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.ServiceModels
{
    public class SessionCheckingServices
    {
        //Thông tin cần lấy của user khi đăng nhập 
        public static int ? userID { get; set; }
        public static string userName { get; set; }
        public static string avatar { get; set; }
        public static void Session(int userID, string userName, string avatar)
        {
            SessionCheckingServices.userID = userID;
            SessionCheckingServices.userName = userName;
            SessionCheckingServices.avatar = avatar;
        }
    }
}
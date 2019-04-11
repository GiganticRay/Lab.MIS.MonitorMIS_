using System;
using System.Linq;
using Lab.MIS.BLL;
using Lab.MIS.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lab.MIS.UnitTest
{
    [TestClass]
    public class UserInfoTest
    {
        public UserInfoService userInfoService = new UserInfoService();
        [TestMethod]
        public void Add()
        {
            
            UserInfo userInfo = new UserInfo();
            userInfo.UserName = "Ray";
            userInfo.UserPwd = "123";
            userInfo.UserAuthority = 1;
            userInfoService.Add(userInfo);
            Console.WriteLine("添加成功");
        }
        [TestMethod]
        public void Get()
        {
            UserInfo tmp = userInfoService.Get(a => a.Id > 0).First();
            Console.WriteLine(string.Format("{0}, {1}, {2}, {3}",tmp.Id, tmp.UserName, tmp.UserPwd, tmp.UserAuthority));
        }

        [TestMethod]
        public void Delete()
        {
            userInfoService.Delete(a => a.Id == 2);
            Console.WriteLine("删除成功");
        }

        [TestMethod]
        public void Update()
        {
            UserInfo userInfo = userInfoService.Get(a => a.Id == 2).First();
            userInfo.UserPwd = "321";
            if (userInfo != null)
            {
                userInfoService.Update(userInfo);
            }
        }

        [TestMethod]
        public void GetPage()
        {
            int TotalPage;
            IQueryable<UserInfo> tmp = userInfoService.GetPage(3, 2, out TotalPage, a => a.Id > 0, a=>a.Id, true);
            Console.WriteLine("总共有{0}页",TotalPage);
            foreach (var userInfo in tmp)
            {
                Console.WriteLine(string.Format("{0}, {1}, {2}, {3}", userInfo.Id, userInfo.UserName, userInfo.UserPwd, userInfo.UserAuthority));
            }
        }
    }
}

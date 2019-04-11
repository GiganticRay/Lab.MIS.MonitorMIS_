using System;
using Lab.MIS.BLL;
using Lab.MIS.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lab.MIS.UnitTest
{
    [TestClass]
    public class MonitorPointInfoTest
    {
        [TestMethod]
        public void Add()
        {
            MonitorPointInfoService monitorPointInfoService = new MonitorPointInfoService();

            MonitorPointInfo monitorPointInfo = new MonitorPointInfo();;
            monitorPointInfo.MonitorId = 1;
            monitorPointInfo.Name = "大树";
            monitorPointInfo.Type = "泥石流";
            monitorPointInfoService.Add(monitorPointInfo);
            Console.WriteLine("添加成功");
        }
    }
}

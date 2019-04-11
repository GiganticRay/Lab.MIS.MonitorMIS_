using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Lab.MIS.BLL;
using Lab.MIS.Model;
using Newtonsoft.Json;
using System.Text;

namespace Lab.MIS.MonitorMIS.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public UserInfoService userInfoService = new UserInfoService();
        public DeviceInfoService deviceInfoService = new DeviceInfoService();
        public PointPictureService pointPictureService = new PointPictureService();
        public MonitorPointInfoService monitorPointInfoService = new MonitorPointInfoService();
        //json序列化的对象
        public JavaScriptSerializer JsSerializer = new JavaScriptSerializer();

        public JsonSerializerSettings setting
        {
            get
            {
                return new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
            }
        }


        public ActionResult HomePage()
        {
            return View();
        }

        public ActionResult Login(UserInfo userInfo)
        {
            IQueryable<UserInfo> userInfos = userInfoService.Get(a => a.UserName == userInfo.UserName && a.UserPwd == userInfo.UserPwd);

            var res = new JsonResult();
            if (userInfos.Count(a => a.Id > 0) == 0)
            {

                res.Data = new { state = false };
            }
            else
            {
                res.Data = new
                {
                    Id = userInfos.First().Id,
                    UserName = userInfos.First().UserName,
                    UserAuthority = userInfos.First().UserAuthority
                };

                HttpCookie UserName = new HttpCookie("UserName", userInfos.First().UserName);
                UserName.Expires = DateTime.Now.AddDays(7);

                HttpCookie UserPwd = new HttpCookie("UserPwd", userInfos.First().UserPwd);
                UserPwd.Expires = DateTime.Now.AddDays(7);

                HttpCookie IsLog = new HttpCookie("IsLog", "true");
                IsLog.Expires = DateTime.Now.AddDays(7);

                Response.Cookies.Add(UserName);
                Response.Cookies.Add(UserPwd);
                Response.Cookies.Add(IsLog);
            }

            return res;
        }

        /// <summary>
        /// 获取返回点击device信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetClickDeviceInfo(int id)
        {
            var res = new JsonResult();
            try
            {
                DeviceInfo deviceInfo = deviceInfoService.Get(a => a.Id == id).First();
                res.Data = JsonConvert.SerializeObject(deviceInfo, setting);
            }
            catch (Exception e)
            {
                res.Data = new { state = false };
            }

            return res;
        }

        /// <summary>
        /// 录入deviceinfo信息
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        public ActionResult EnteringDeviceInfo(DeviceInfo deviceInfo)
        {
            var res = new JsonResult();
            try
            {
                DeviceInfo getAddDeviceInfo = deviceInfoService.Add(deviceInfo);
                res.Data = new { state = getAddDeviceInfo.Id };
            }
            catch (Exception e)
            {
                res.Data = new { state = false };
            }

            return res;
        }

        /// <summary>
        /// 录入monitorInfo信息
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        public ActionResult EnteringMonitorPointInfo(MonitorPointInfo monitorPointInfo)
        {
            var res = new JsonResult();
            try
            {
                monitorPointInfoService.Add(monitorPointInfo);
                res.Data = new { state = true };
            }
            catch (Exception e)
            {
                res.Data = new { state = false };
            }

            return res;
        }

        /// <summary>
        /// 获取MonitorInfos和monitorInfos对应的deviceInfo信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMonitorInfos()
        {
            List<MonitorPointInfo> monitorPointInfos = monitorPointInfoService.Get(a => a.Id > 0).ToList();
            var res = new JsonResult();
            res.Data = JsonConvert.SerializeObject(monitorPointInfos, setting);
            return res;
        }
        /// <summary>
        /// 获取所有检测设备的信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAllDevicePoints()
        {

            List<DeviceInfo> getaAllDeviceInfo = deviceInfoService.Get(a => a.Id > 0).ToList();

            var res = new JsonResult();
            res.Data = JsonConvert.SerializeObject(getaAllDeviceInfo, setting);
            return res;
        }


        public ActionResult GetDiseaseInfo(string arrayId, string beforeTime, string endTime)
        {
            string GetUrl = "http://47.92.125.37/mudrock/user/getResult" + "?arrayId=" + arrayId + "&beforeTime=" + beforeTime + "&endTime=" + endTime;
            string Jsonstr = JsonToTableClass.GetJson(GetUrl);
            return Content(Jsonstr);
        }

        /// <summary>
        /// 根据id删除检测设备
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public ActionResult DeleteDevice(int id = 0)
        {
            try
            {
                //获取图片集合
                List<PointPicture> picList = pointPictureService.Get(b => b.DeviceInfoId == id).ToList();
                //删除图片
                foreach (var item in picList)
                {

                    //相对路径
                    string filePath = item.PicPath;
                    //图片绝对路径
                    string absolutePath = Server.MapPath(filePath);
                    //判断文件是否存在
                    if (System.IO.File.Exists(absolutePath))
                    {
                        //如果文件存在，则删除
                        System.IO.File.Delete(absolutePath);
                    }
                    //从数据库中山粗话
                    pointPictureService.Delete(g => g.Id == item.Id);
                }
            }
            catch (Exception)
            {
                return Content("-1");
            }
            //删除监测设备点
            return Content(deviceInfoService.Delete(a => a.Id == id).ToString());
        }
        /// <summary>
        /// 保存检测设备
        /// </summary>
        /// <param name="deviceinfo"></param>
        /// <returns></returns>
        public ActionResult SaveDevice(DeviceInfo deviceinfo)
        {
            bool getResult = deviceInfoService.Update(deviceinfo);
            return Content(getResult.ToString());
        }

        /// <summary>
        /// 通过id获取监测阵列的类型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetOneMonitorPointInfo(int id = 13)
        {
            MonitorPointInfo newMoitor = monitorPointInfoService.Get(a => a.MonitorId == id).First();

            string getMonitorType = newMoitor.Type.Length > 0 ? newMoitor.Type : "错误，未知";
            return Content(getMonitorType);
        }
        /// <summary>
        /// 获取未满3个检测设备的检测阵列
        /// </summary>
        /// <returns></returns>
        public ActionResult GetNewMonitorInfos()
        {
            List<MonitorPointInfo> monitorPointInfos = monitorPointInfoService.Get(a => a.Id > 0).ToList();
            //List<MonitorPointInfo> newPointInfo = new List<MonitorPointInfo>();
            //foreach (var item in monitorPointInfos)
            //{
            //    int getDeviceMonitorId = item.MonitorId;

            //    int count = deviceInfoService.Get(a => a.MonitorPointInfoId == getDeviceMonitorId).Count();

            //    //判断是否有三个设备
            //    if (count < 3)
            //    {
            //        newPointInfo.Add(item);
            //    }
            //}
            var res = new JsonResult();
            res.Data = JsonConvert.SerializeObject(monitorPointInfos, setting);
            return res;
        }
        /// <summary>
        /// 获取树状结构的数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTreeJson()
        {
            StringBuilder backData = new StringBuilder();

            backData.Append("[{");
            backData.Append("\"tags\":[\"-1\"],\"text\":\"滑坡\",\"nodes\":[");
            List<DeviceInfo> newDeviceList = deviceInfoService.Get(a => a.MonitorType == "滑坡").ToList();
            foreach (var item in newDeviceList)
            {
                backData.Append("{");
                backData.AppendFormat("\"tags\":[\"{0}\"],", item.Id);
                backData.AppendFormat("\"text\":[\"{0}\"]", item.DeviceName);
                backData.Append("},");
            }
            //将最后一个“,”移除
            backData.Remove(backData.Length - 1, 1);
            backData.Append("]");



            backData.Append("},");



            backData.Append("{");
            backData.Append("\"tags\":[\"-1\"],\"text\":\"泥石流\",\"nodes\":[");
            List<DeviceInfo> newDeviceListSecond = deviceInfoService.Get(a => a.MonitorType == "泥石流").ToList();
            foreach (var item in newDeviceListSecond)
            {
                backData.Append("{");
                backData.AppendFormat("\"tags\":[\"{0}\"],", item.Id);
                backData.AppendFormat("\"text\":[\"{0}\"]", item.DeviceName);
                backData.Append("},");
            }
            //将最后一个“,”移除
            backData.Remove(backData.Length - 1, 1);
            backData.Append("]");

            backData.Append("}]");

            return Content(backData.ToString());
        }
        /// <summary>
        /// 通过id获取一个检测设备的信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOneDevice(int id)
        {
            List<DeviceInfo> getaAllDeviceInfo = deviceInfoService.Get(a => a.Id == id).ToList();

            var res = new JsonResult();
            res.Data = JsonConvert.SerializeObject(getaAllDeviceInfo, setting);
            return res;

        }
        /// <summary>
        /// 根据id查询监测阵列的三个监测设备信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetDeviceInfoByMonitorId(int id)
        {
            List<DeviceInfo> tmp = deviceInfoService.Get(a => a.MonitorPointInfoId == id).ToList();
            var res = new JsonResult();
            res.Data = JsonConvert.SerializeObject(tmp, setting);
            return res;
        }

        /// <summary>
        /// 获取模糊查询的结果
        /// </summary>
        /// <param name="SearchContent"></param>
        /// <returns></returns>
        public ActionResult GetVagueSearch(string SearchContent)
        {
            List<DeviceInfo> tmp = deviceInfoService.Get(a => a.DeviceName.IndexOf(SearchContent) >= 0).ToList();
            var res = new JsonResult();
            res.Data = JsonConvert.SerializeObject(tmp, setting);
            return res;
        }

        /// <summary>
        /// 录入图片路径信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="imgPaths"></param>
        /// <returns></returns>
        public ActionResult EnteringPics(int id, string imgPaths)
        {
            var res = new JsonResult();
            try
            {

                string[] getImgPathArry = imgPaths.Split(';');

                foreach (var item in getImgPathArry)
                {
                    if (item.Length > 0)
                    {
                        PointPicture newPic = new PointPicture();
                        newPic.DeviceInfoId = id;
                        newPic.PicPath = item;
                        pointPictureService.Add(newPic);
                    }

                }
                res.Data = new { state = true };
            }
            catch (Exception)
            {

                res.Data = new { state = false };
            }

            return res;
        }
        /// <summary>
        /// 通过设备点的id获取其图片
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetPicPathById(int id)
        {
            var res = new JsonResult();
            List<PointPicture> getPictureList = pointPictureService.Get(a => a.DeviceInfoId == id).ToList();
            res.Data = JsonConvert.SerializeObject(getPictureList, setting);


            return res;
        }
        /// <summary>
        /// 更新图片
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult editUploadImgs(int DevieceID, string path)
        {

            //将新增的图片目录录入数据库
            PointPicture newPic = new PointPicture();
            newPic.DeviceInfoId = DevieceID;
            newPic.PicPath = path;
            PointPicture addPic = pointPictureService.Add(newPic);

            var res = new JsonResult();
            if (addPic.Id > 0)
            {
                res.Data = new { msg = true };
            }
            else
            {
                res.Data = new { msg = false };
            }
            return res;
        }
        /// <summary>
        /// 通过id删除已经存在的图片
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public ActionResult DeleteExistImgs(int key)
        {
            //string getId = context.Request["key"];

            //int idNum = Convert.ToInt32(getId);
            int idNum = key;
            var res = new JsonResult();
            try
            {
                PointPicture deletePic = pointPictureService.Get(a => a.Id == idNum).First();
                //相对路径
                string filePath = deletePic.PicPath;
                //图片绝对路径
                string absolutePath = Server.MapPath(filePath);
                //判断文件是否存在
                if (System.IO.File.Exists(absolutePath))
                {
                    //如果文件存在，则删除
                    System.IO.File.Delete(absolutePath);
                }
                //从数据库中删除
                if (pointPictureService.Delete(a => a.Id == idNum) > 0)
                {
                    res.Data = new { msg = true };
                }
                else
                {
                    res.Data = new { msg = false };
                }
            }
            catch (Exception)
            {

                res.Data = new { msg = false };
            }
            return res;
        }

        public ActionResult Delete_Entering_Exist_imgs(string key)
        {
            var res = new JsonResult();
            res.Data = new { res_path = key };
            return res;
        }


        #region 测试方法
        public ActionResult TestGetDiseaseInfo(string arrayId, string beforeTime, string endTime)
        {
            // 测试的时候因为前台请求次数是根据监测阵列、监测类型来判断的、所以会请求多次、自然而然这里会显示多次
            string Jsonstr =
                "\"[{ArrayID:'14', CenterLon:'109.3604',CenterLat:'30.9627',Lon:'109.3694',Lat:'30.9672',AverageAngle:'60.0',Type:'滑坡',Grade:'2',RecTime:'2017-12-01'}]\"";
            return Content(Jsonstr);
        }
        #endregion
    }
}

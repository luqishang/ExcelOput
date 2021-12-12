using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Management;
using System.IO;
using HACCPExtender.Models;
using System.Linq;
using System.Data.Entity.Infrastructure;
using HACCPExtender.Controllers.Common;
using static HACCPExtender.Controllers.Common.CommonConstants;
using System.Data.Entity;

namespace HACCPExtender
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo(Server.MapPath("~/log4net.config")));

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Session_Start()
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("session_start");
            System.Diagnostics.Debug.WriteLine(Request.RawUrl);
#else
            if (Request.RawUrl.IndexOf("/Init/") < 0)
            {
                if (Session["DISPMODE"] == null)
                {
                    // エラー画面へ遷移(セッションタイムアウト)
                    //Response.Redirect("/SessionError.html");
                }
            }
# endif
        }

        protected void Application_BeginRequest(Object source, EventArgs e)
        {
            HttpApplication app = (HttpApplication)source;
            var Host = FirstRequestInitialisation.Initialise(app.Context);
            //this.SetHostName(Host);
        }

        private void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            var httpException = ex as HttpException ?? ex.InnerException as HttpException;
            if (httpException == null) return;

            if (httpException.WebEventCode == WebEventCodes.RuntimeErrorPostTooLarge)
            {
                //handle the error
                Response.Redirect("/FileSizeError.html");
            }
        }

        private void SetHostName(string host)
        {
            using (var context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        var insModel = new GeneralPurposeM();
                        var updModel = new GeneralPurposeM();

                        var general = from ge in context.GeneralPurposeMs
                                      where ge.KEY == EnvironmentKey.KEY_HOSTNAME
                                      select ge;

                        if (general.Count() > 0)
                        {
                            updModel = general.FirstOrDefault();
                            updModel.VALUE1 = host;

                            context.GeneralPurposeMs.Attach(updModel);
                            context.Entry(updModel).State = EntityState.Modified;
                            context.SaveChanges();
                        }
                        else
                        {
                            insModel.KEY = EnvironmentKey.KEY_HOSTNAME;
                            insModel.VALUE1 = host;

                            context.GeneralPurposeMs.Add(insModel);
                            context.SaveChanges();
                        }

                        tran.Commit();
                    }
                    catch (DbUpdateException ex)
                    {
                        // ロールバック
                        tran.Rollback();
                        LogHelper.Default.WriteError(ex.Message, ex);
                    }
                    catch (Exception ex)
                    {
                        // ロールバック
                        tran.Rollback();
                        LogHelper.Default.WriteError(ex.Message, ex);
                    }
                }
            }
        }

        static class FirstRequestInitialisation
        {
            private static string Host = null;
            private static Object s_lock = new object();

            public static string Initialise(HttpContext context)
            {
                if (string.IsNullOrEmpty(Host))
                {
                    lock (s_lock)
                    {
                        if (string.IsNullOrEmpty(Host))
                        {
                            var uri = context.Request.Url;
                            Host = uri.GetLeftPart(UriPartial.Authority);
                        }
                    }
                }
                return Host;
            }
        }
    }
}

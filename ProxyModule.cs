using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web;

namespace HttpProxy
{
    public class ProxyModule : IHttpModule
    {
        public void Dispose()
        {  //对此方法先不实现
        }

        public void Init(HttpApplication context)
        {
            //订阅HttpApplication中BeginRequst事件
            context.BeginRequest += Context_BeginRequest;
            //订阅HttpApplication中EndRequest事件
            context.EndRequest += Context_EndRequest;
        }

        private void Context_EndRequest(object sender, EventArgs e)
        {
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            var req = application.Context.Request;
            HttpResponse response = application.Context.Response;

            var myreq = Transfer(req);
            using (var myresponse = myreq.GetResponse())
            {
                WriteResponse(myresponse as HttpWebResponse, response);
            }
            response.End();
        }

        private void WriteResponse(HttpWebResponse from, HttpResponse to)
        {
            foreach (var item in from.Headers.AllKeys)
            {
                var val = from.Headers[item];
                Logger.Trace($"WriteResponse set header: key {item} value {val}", GetType());

                to.Headers.Add(item, val);
            }

            to.ContentType = from.ContentType;
            Logger.Trace($"WriteResponse set ContentType: {from.ContentType}", GetType());
            to.StatusCode = (int)from.StatusCode;
            Logger.Trace($"WriteResponse set StatusCode: {(int)from.StatusCode}", GetType());

            string _responseContent = "";
            var responseStream = from.GetResponseStream();
            if (responseStream != null)
            {
                using (var reader = new StreamReader(responseStream))
                {
                    _responseContent = reader.ReadToEnd();
                }
                if (_responseContent == null)
                {
                    _responseContent = "";
                }
            }
            else
            {
                _responseContent = "";
            }

            to.Write(_responseContent);
        }

        private HttpWebRequest Transfer(HttpRequest hr)
        {
            var to_server = ConfigurationManager.AppSettings["to_server"];

            var url = to_server + hr.RawUrl;

            Logger.Trace($"transfer : from {url} to {to_server}", GetType());

            var httpOp = new HttpRequestOptions ();
            httpOp.Method = hr.HttpMethod;
            Logger.Trace($"transfer set method ：{hr.HttpMethod}", GetType());
            httpOp.ContentType = hr.ContentType;
            Logger.Trace($"transfer set ContentType ：{hr.ContentType}", GetType());

            httpOp.Headers = new List<KeyValuePair<string, string>>();
            foreach (var item in hr.Headers.AllKeys)
            {
                var val = hr.Headers[item];
                Logger.Trace($"transfer add header: key:{item} value:{val}", GetType());
                if (!MyHttpRequest.ExHeaders.Contains(item))
                {
                    httpOp.Headers.Add(new KeyValuePair<string, string>(item, val));
                }
            }
            return MyHttpRequest.CreateRequest(url, httpOp);
        }
    }
}
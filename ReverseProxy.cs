using HttpProxy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace HttpProxy2
{
    public class ReverseProxy : IHttpHandler,
                    System.Web.SessionState.IRequiresSessionState
    {
        /// <summary>
        /// Method calls when client request the server
        /// </summary>
        /// <param name="context">HTTP context for client</param>
        public void ProcessRequest(HttpContext context)
        {
            var req = context.Request;
            HttpResponse response = context.Response;

            var myresp = (HttpWebResponse)Transfer(req);
            WriteResponse(myresp, response);

            response.End();
        }

        public bool IsReusable
        {
            get { return true; }
        }

        private void WriteResponse(HttpWebResponse from, HttpResponse to)
        {
            foreach (var item in from.Headers.AllKeys)
            {
                var val = from.Headers[item];
                if (!MyHttpRequest.ExHeaders.Contains(item))
                {
                    Logger.Trace($"WriteResponse set header: key {item} value {val}", GetType());
                    //to.Headers.Add(item, val);
                }
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
            Logger.Trace($"WriteResponse write ResponseContent: {_responseContent}", GetType());
            to.Write(_responseContent);
        }

        private WebResponse Transfer(HttpRequest hr)
        {
            var to_server = ConfigurationManager.AppSettings["to_server"];

            var url = to_server + hr.RawUrl;

            Logger.Trace($"transfer : from {hr.Url} to {to_server}", GetType());

            var httpOp = new HttpRequestOptions();
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
            if (hr.AcceptTypes != null)
            {
                httpOp.Accept = string.Join(",", hr.AcceptTypes);
            }
            
            byte[] byts = new byte[hr.InputStream.Length];
            hr.InputStream.Read(byts, 0, byts.Length);
            httpOp.PostBody = byts;

            var requestBody = System.Text.Encoding.UTF8.GetString(byts);
            Logger.Trace($"transfer request body: {requestBody}", GetType());
            WebResponse response = null;
            try
            {
                var req = MyHttpRequest.CreateRequest(url, httpOp);
                if (httpOp.Method == "POST" || httpOp.Method == "PUT")
                {
                    if (httpOp.PostBody != null)
                    {
                        req.ContentLength = httpOp.PostBody.Length;
                        using (var requestStream = req.GetRequestStream())
                        {
                            requestStream.Write(httpOp.PostBody, 0, httpOp.PostBody.Length);
                        }
                    }
                }
            
                response = req.GetResponse();

            }
            catch (Exception e)
            {
                Logger.Error($"transfer a request occous err . {url}", GetType(), e);
                throw;
            }

            return response;
        }

    }
}
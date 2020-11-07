using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HttpProxy
{
    public class HttpRequestOptions
    {
        public byte[] PostBody { get; set; }

        public List<KeyValuePair<string, string>> Headers { get; set; }

        public string Method { get; set; }

        public string ContentType { get; set; }

        public string Accept { get; set; }

        public string Host { get; set; }

        public IWebProxy Proxy { get; set; }
    }

    public class HttpRequestResult : IDisposable
    {

        public HttpRequestResult(HttpWebResponse response, Exception ex)
        {
            this.Response = response;
            this.Exception = ex;
            if (this.Response != null)
            {
                this.StatusCode = this.Response.StatusCode;
            }
        }

        public HttpWebResponse Response { get; private set; }

        private string _responseContent;
        public string ResponseContent
        {
            get
            {
                if (_responseContent != null)
                {
                    return _responseContent;
                }
                if (Response == null)
                {
                    _responseContent = "";
                    return _responseContent;
                }

                var responseStream = Response.GetResponseStream();
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

                return _responseContent;
            }

            private set { }
        }

        public HttpStatusCode? StatusCode { get; private set; }

        public Exception Exception { get; private set; }

        public void Dispose()
        {
            if (Response != null)
            {
                Response.Dispose();
            }
        }
    }


    public class MyHttpRequest
    {
        public static List<string> ExHeaders = new List<string>()
        {
            "Connection",
            "Content-Type",
            "Host",
            "User-Agent",
            "Referer",
            "Content-Length",
            "Transfer-Encoding",
            "Range",
            "Request-Id",
            "RequestId",
            "RequestID",
            "request-id",
            "requestid",
            "Accept",
            "If-Modified-Since",
            "Expect"
        };

        public static HttpWebRequest CreateRequest(string url, HttpRequestOptions options)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = options.Method.IsNullOrEmpty() ? "GET" : options.Method;
            if (!options.ContentType.IsNullOrEmpty())
            {
                request.ContentType = options.ContentType;
            }
            if (!options.Host.IsNullOrEmpty())
            {
                request.Host = options.Host;
            }
            if (!options.Accept.IsNullOrEmpty())
            {
                //request.Accept = options.Accept;
            }
            if (options.Proxy != null)
            {
                request.Proxy = options.Proxy;
            }
            //add header
            if (options.Headers != null)
            {
                foreach (var keyValuePair in options.Headers)
                {
                    request.Headers.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            if (options.Method == "POST")
            {
                request.ContentLength = 0;
            }

            return request;
        }

        /// <summary>
        /// 发送一个HTTP请求，默认为GET请求
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="options">请求的具体配置</param>
        /// <returns></returns>
        public static HttpRequestResult Request(string url, HttpRequestOptions options)
        {
            var request = CreateRequest(url, options);

            //start request
            try
            {
                //add body
                if (options.PostBody != null)
                {
                    request.ContentLength = options.PostBody.Length;
                    using (var requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(options.PostBody, 0, options.PostBody.Length);
                    }
                }

                var response = (HttpWebResponse)request.GetResponse();
                return new HttpRequestResult(response, null);
            }
            catch (WebException ex)
            {
                var response = (HttpWebResponse)ex.Response;
                if (response == null)
                {
                    return new HttpRequestResult(null, ex);
                }
                else
                {
                    return new HttpRequestResult(response, ex);
                }
            }
            catch (Exception ex)
            {
                return new HttpRequestResult(null, ex);
            }

        }

        /// <summary>
        /// Get的快捷形式
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpRequestResult Get(string url)
        {
            return Request(url, new HttpRequestOptions() { Method = "GET" });
        }

        /// <summary>
        /// Get的快捷形式
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpRequestResult Get(string url, List<KeyValuePair<string, string>> headers)
        {
            return Request(url, new HttpRequestOptions() { Method = "GET", Headers = headers });
        }

        /// <summary>
        /// Post的快捷形式,ContentType = application/json
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postBody"></param>
        /// <returns></returns>
        public static HttpRequestResult Post(string url, object postBody)
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(postBody));
            return Request(url, new HttpRequestOptions() { Method = "POST", ContentType = "application/json", PostBody = data });
        }

        /// <summary>
        /// Post的快捷形式,ContentType = application/json
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postBody"></param>
        /// <returns></returns>
        public static HttpRequestResult Post(string url, List<KeyValuePair<string, string>> headers, object postBody)
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(postBody));
            return Request(url, new HttpRequestOptions() { Method = "POST", ContentType = "application/json", PostBody = data, Headers = headers });
        }

        /// <summary>
        /// 发送一个异步HTTP请求,默认为GET请求
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="options">请求的具体配置</param>
        /// <returns></returns>
        public static Task<HttpRequestResult> RequestAsync(string url, HttpRequestOptions options)
        {
            var task = Task.Run(async () =>
            {
                var request = CreateRequest(url, options);

                //start request
                try
                {
                    //add body
                    if (options.PostBody != null)
                    {
                        request.ContentLength = options.PostBody.Length;
                        using (var requestStream = await request.GetRequestStreamAsync())
                        {
                            requestStream.Write(options.PostBody, 0, options.PostBody.Length);
                        }
                    }

                    var response = (HttpWebResponse)await request.GetResponseAsync();
                    return new HttpRequestResult(response, null);
                }
                catch (WebException ex)
                {
                    var response = (HttpWebResponse)ex.Response;
                    if (response == null)
                    {
                        return new HttpRequestResult(null, ex);
                    }
                    else
                    {
                        return new HttpRequestResult(response, ex);
                    }
                }
                catch (Exception ex)
                {
                    return new HttpRequestResult(null, ex);

                }
            });

            return task;

        }

        /// <summary>
        /// Get的快捷形式
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Task<HttpRequestResult> GetAsync(string url)
        {
            return RequestAsync(url, new HttpRequestOptions() { Method = "GET" });
        }

        /// <summary>
        /// Post的快捷形式,ContentType = application/json
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postBody"></param>
        /// <returns></returns>
        public static Task<HttpRequestResult> PostAsync(string url, object postBody)
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(postBody));
            return RequestAsync(url, new HttpRequestOptions() { Method = "POST", ContentType = "application/json", PostBody = data });
        }

        public static T RestPost<T>(string url, List<KeyValuePair<string, string>> headers, object postBody) where T : class
        {
            using (var result = Post(url, headers, postBody))
            {
                if (result.Exception != null)
                {
                    throw result.Exception;
                }
                else
                {
                    var t = JsonConvert.DeserializeObject<T>(result.ResponseContent);

                    return t;
                }
            }
        }

        public static T RestPut<T>(string url, List<KeyValuePair<string, string>> headers, object postBody) where T : class
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(postBody));
            using (var result = Request(url, new HttpRequestOptions() { Method = "PUT", ContentType = "application/json", PostBody = data, Headers = headers }))
            {
                if (result.Exception != null)
                {
                    throw result.Exception;
                }
                else
                {
                    var t = JsonConvert.DeserializeObject<T>(result.ResponseContent);

                    return t;
                }
            }
        }

        public static T RestGet<T>(string url, List<KeyValuePair<string, string>> headers) where T : class
        {
            using (var result = Get(url, headers))
            {
                if (result.Exception != null)
                {
                    throw result.Exception;
                }
                else
                {
                    var t = JsonConvert.DeserializeObject<T>(result.ResponseContent);

                    return t;
                }
            }
        }

        public static T RestPost<T>(string url, object postBody) where T : class
        {
            using (var result = Post(url, postBody))
            {
                if (result.Exception != null)
                {
                    throw result.Exception;
                }
                else
                {
                    var t = JsonConvert.DeserializeObject<T>(result.ResponseContent);

                    return t;
                }
            }
        }

        public static T RestPut<T>(string url, object postBody) where T : class
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(postBody));
            using (var result = Request(url, new HttpRequestOptions() { Method = "PUT", ContentType = "application/json", PostBody = data }))
            {
                if (result.Exception != null)
                {
                    throw result.Exception;
                }
                else
                {
                    var t = JsonConvert.DeserializeObject<T>(result.ResponseContent);

                    return t;
                }
            }
        }

        public static T RestGet<T>(string url) where T : class
        {
            using (var result = Get(url))
            {
                if (result.Exception != null)
                {
                    throw result.Exception;
                }
                else
                {
                    var t = JsonConvert.DeserializeObject<T>(result.ResponseContent);

                    return t;
                }
            }
        }
    }
}
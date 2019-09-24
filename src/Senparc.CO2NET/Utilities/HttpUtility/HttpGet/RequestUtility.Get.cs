﻿#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2019 Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2019 Senparc

    文件名：RequestUtility.Get.cs
    文件功能描述：获取请求结果（Get）


    创建标识：Senparc - 20171006

    修改描述：移植Get方法过来

    修改标识：Senparc - 20190429
    修改描述：v0.7.0 优化 HttpClient，重构 RequestUtility（包括 Post 和 Get），引入 HttpClientFactory 机制


----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Senparc.CO2NET.Helpers;
#if NET35 || NET40 || NET45
using System.Web;
#else
using System.Net.Http;
using System.Net.Http.Headers;
#endif
#if NETSTANDARD2_0 || NETSTANDARD2_1
using Microsoft.AspNetCore.Http;
using Senparc.CO2NET.WebProxy;
#endif

namespace Senparc.CO2NET.HttpUtility
{
    /// <summary>
    /// HTTP 请求工具类
    /// </summary>
    public static partial class RequestUtility
    {
        #region 公用静态方法

#if NET35 || NET40 || NET45
        /// <summary>
        /// .NET 4.5 版本的HttpWebRequest参数设置
        /// </summary>
        /// <returns></returns>
        private static HttpWebRequest HttpGet_Common_Net45(string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
            string refererUrl = null, bool useAjax = false, int timeOut = Config.TIME_OUT)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = timeOut;
            request.Proxy = _webproxy;
            if (cer != null)
            {
                request.ClientCertificates.Add(cer);
            }

            if (cookieContainer != null)
            {
                request.CookieContainer = cookieContainer;
            }

            HttpClientHeader(request, refererUrl, useAjax, null, timeOut);//设置头信息

            return request;
        }
#endif

#if NETSTANDARD2_0 || NETSTANDARD2_1
        /// <summary>
        /// .NET Core 版本的HttpWebRequest参数设置
        /// </summary>
        /// <returns></returns>
        private static HttpClient HttpGet_Common_NetCore(string url, CookieContainer cookieContainer = null,
            Encoding encoding = null, X509Certificate2 cer = null,
            string refererUrl = null, bool useAjax = false, int timeOut = Config.TIME_OUT)
        {
            var handler = HttpClientHelper.GetHttpClientHandler(cookieContainer, RequestUtility.SenparcHttpClientWebProxy, DecompressionMethods.GZip);

            if (cer != null)
            {
                handler.ClientCertificates.Add(cer);
            }

            HttpClient httpClient = SenparcDI.GetRequiredService<SenparcHttpClient>().Client;
            HttpClientHeader(httpClient, refererUrl, useAjax, null, timeOut);

            return httpClient;
        }
#endif

        #endregion

        #region 同步方法

        /// <summary>
        /// 使用Get方法获取字符串结果（没有加入Cookie）
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string HttpGet(string url, Encoding encoding = null)
        {
#if NET35 || NET40 || NET45
            WebClient wc = new WebClient();
            wc.Proxy = _webproxy;
            wc.Encoding = encoding ?? Encoding.UTF8;
            return wc.DownloadString(url);
#else
            var handler = HttpClientHelper.GetHttpClientHandler(null, SenparcHttpClientWebProxy, DecompressionMethods.GZip);


            HttpClient httpClient = SenparcDI.GetRequiredService<SenparcHttpClient>().Client;

            return httpClient.GetStringAsync(url).Result;
#endif
        }

        /// <summary>
        /// 使用Get方法获取字符串结果（加入Cookie）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="refererUrl">referer参数</param>
        /// <param name="useAjax">是否使用Ajax</param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static string HttpGet(string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
            string refererUrl = null, bool useAjax = false, int timeOut = Config.TIME_OUT)
        {
#if NET35 || NET40 || NET45
            HttpWebRequest request = HttpGet_Common_Net45(url, cookieContainer, encoding, cer, refererUrl, useAjax, timeOut);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (cookieContainer != null)
            {
                response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            }

            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader myStreamReader = new StreamReader(responseStream, encoding ?? Encoding.GetEncoding("utf-8")))
                {
                    string retString = myStreamReader.ReadToEnd();
                    return retString;
                }
            }
#else

            var httpClient = HttpGet_Common_NetCore(url, cookieContainer, encoding, cer, refererUrl, useAjax, timeOut);

            var response = httpClient.GetAsync(url).GetAwaiter().GetResult();//获取响应信息

            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//设置 Cookie

            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
#endif
        }

#if NET35 || NET40 || NET45

        /// <summary>
        /// 获取HttpWebResponse或HttpResponseMessage对象，本方法通常用于测试）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer"></param>
        /// <param name="refererUrl"></param>
        /// <param name="useAjax"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static HttpWebResponse HttpResponseGet(string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
    string refererUrl = null, bool useAjax = false, int timeOut = Config.TIME_OUT)
        {
            HttpWebRequest request = HttpGet_Common_Net45(url, cookieContainer, encoding, cer, refererUrl, useAjax, timeOut);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (cookieContainer != null)
            {
                response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            }

            return response;
        }
#else
        /// <summary>
        /// 获取HttpWebResponse或HttpResponseMessage对象，本方法通常用于测试）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer"></param>
        /// <param name="refererUrl"></param>
        /// <param name="useAjax">是否使用Ajax请求</param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static HttpResponseMessage HttpResponseGet(string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
   string refererUrl = null, bool useAjax = false, int timeOut = Config.TIME_OUT)
        {
            var httpClient = HttpGet_Common_NetCore(url, cookieContainer, encoding, cer, refererUrl, useAjax, timeOut);
            var task = httpClient.GetAsync(url);
            HttpResponseMessage response = task.Result;

            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//设置 Cookie

            return response;
        }

#endif


        #endregion

#if !NET35 && !NET40
        #region 异步方法

        /// <summary>
        /// 使用Get方法获取字符串结果（没有加入Cookie）
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> HttpGetAsync(string url, Encoding encoding = null)
        {
#if NET35 || NET40 || NET45
            WebClient wc = new WebClient();
            wc.Proxy = _webproxy;
            wc.Encoding = encoding ?? Encoding.UTF8;
            return await wc.DownloadStringTaskAsync(url).ConfigureAwait(false);
#else
            var handler = new HttpClientHandler
            {
                UseProxy = SenparcHttpClientWebProxy != null,
                Proxy = SenparcHttpClientWebProxy,
            };

            HttpClient httpClient = SenparcDI.GetRequiredService<SenparcHttpClient>().Client;
            return await httpClient.GetStringAsync(url).ConfigureAwait(false);
#endif

        }

        /// <summary>
        /// 使用Get方法获取字符串结果（加入Cookie）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="timeOut"></param>
        /// <param name="refererUrl">referer参数</param>
        /// <returns></returns>
        public static async Task<string> HttpGetAsync(string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
            string refererUrl = null, bool useAjax = false, int timeOut = Config.TIME_OUT)
        {
#if NET35 || NET40 || NET45
            HttpWebRequest request = HttpGet_Common_Net45(url, cookieContainer, encoding, cer, refererUrl, useAjax, timeOut);

            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync().ConfigureAwait(false));

            if (cookieContainer != null)
            {
                response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            }

            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader myStreamReader = new StreamReader(responseStream, encoding ?? Encoding.GetEncoding("utf-8")))
                {
                    string retString = await myStreamReader.ReadToEndAsync().ConfigureAwait(false);
                    return retString;
                }
            }
#else
            var httpClient = HttpGet_Common_NetCore(url, cookieContainer, encoding, cer, refererUrl, useAjax, timeOut);

            var response = await httpClient.GetAsync(url).ConfigureAwait(false);//获取响应信息

            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//设置 Cookie

            var retString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return retString;
#endif
        }

        #endregion
#endif
    }
}

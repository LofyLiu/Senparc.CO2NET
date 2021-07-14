﻿using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Senparc.CO2NET
{
    /// <summary>
    /// 自动绑定属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ApiBindAttribute : Attribute
    {
        /// <summary>
        /// 目录（平台类型），用于输出 API 的 Url 时分组
        /// </summary>
        public string Category { private get; set; }
        /// <summary>
        /// 平台内唯一名称（请使用宇宙唯一名称，如： [namespace].[ClassName].[MethodName]）
        /// </summary>
        public string Name { private get; set; }

        public ApiRequestMethod ApiRequestMethod { get; set; }

        /// <summary>
        /// ApiBindAttributes 构造函数
        /// </summary>
        public ApiBindAttribute() { }

        /// <summary>
        /// 自动绑定属性
        /// </summary>
        /// <param name="category">目录（平台类型），用于输出 API 的 Url 时分组</param>
        /// <param name="name">平台内唯一名称（如使用 PlatformType.General，请使用宇宙唯一名称）</param>
        /// <param name="apiRequestMethod">当前 API 请求的类型，如果为 null，则使用本次引擎全局定义的 </param>
        public ApiBindAttribute(string category, string name) : this(category, name, WebApi.ApiRequestMethod.GlobalDefault)
        {
        }

        /// <summary>
        /// 自动绑定属性
        /// </summary>
        /// <param name="category">目录（平台类型），用于输出 API 的 Url 时分组</param>
        /// <param name="name">平台内唯一名称（如使用 PlatformType.General，请使用宇宙唯一名称）</param>
        /// <param name="apiRequestMethod">当前 API 请求的类型，如果为 null，则使用本次引擎全局定义的 </param>
        public ApiBindAttribute(string category, string name, ApiRequestMethod apiRequestMethod)
        {
            Category = category;
            Name = name;
            ApiRequestMethod = apiRequestMethod;
        }

        /// <summary>
        /// 获取不为空的可显示、使用的Category名称
        /// </summary>
        /// <param name="realAssemblyName">真实的当前程序集的名称（AssemblyName.Name）</param>
        /// <returns></returns>
        public string GetCategoryName(string realAssemblyName)
        {
            return Category ?? realAssemblyName;
        }

        /// <summary>
        /// 获取不为空的可显示、使用的Category名称
        /// </summary>
        /// <param name="realAssemblyName">真实的当前程序集的名称（AssemblyName.Name）</param>
        /// <returns></returns>
        public string GetCategoryName(MethodInfo methodInfo)
        {
            return GetCategoryName(methodInfo.DeclaringType.Assembly.GetName().Name);
        }

        public string GetName(MethodInfo methodInfo)
        {
            return Name ?? $"{methodInfo.DeclaringType.Name}.{methodInfo.Name}";
        }

    }
}

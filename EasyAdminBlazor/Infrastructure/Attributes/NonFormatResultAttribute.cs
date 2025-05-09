using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyAdminBlazor.Infrastructure.Attributes;

/// <summary>
/// 不格式化结果数据
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class NonFormatResultAttribute : Attribute
{
}
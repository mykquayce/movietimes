using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MovieTimes.Api.WebApplication
{
	public class PlainTextOutputFormatter : TextOutputFormatter
	{
		public PlainTextOutputFormatter()
		{
			SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/plain"));
			SupportedEncodings.Add(Encoding.UTF8);
			SupportedEncodings.Add(Encoding.Unicode);
		}

		public async override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
		{
			var sb = new StringBuilder();

			IEnumerable<PropertyInfo>? properties = default;

			void f(object o)
			{
				properties ??= o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
				var values = properties.Select(p => p.GetValue(o));
				var line = string.Join("\t", values);
				sb.AppendLine(line);
			}

			if (context.Object is IEnumerable enumerable)
			{
				foreach (var item in enumerable)
				{
					f(item!);
				}
			}
			else
			{
				f(context.Object);
			}

			using var writer = context.WriterFactory(context.HttpContext.Response.Body, selectedEncoding);

			await writer.WriteAsync(sb.ToString());
		}
	}
}

// =====================================================
// =
// =   该文件为工具生成，如有修改会被覆盖
// =
// =====================================================
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Data.Config
{
	public static class ExampleConfig
	{
		public static string FileName => "example";

		private static Example data;

		public static bool InitData(string json)
		{
			try
			{
				data = JsonConvert.DeserializeObject<Example>(json);
			}
			catch (Exception e)
			{
				Debug.Log($"配置解析失败：{e}");
				return false;
			}

			return true;
		}

		public static List<Food> FoodList => data.FoodList;

	}

	public class Example : IData
	{
		[JsonProperty("food")]
		public readonly List<Food> FoodList;

	}

	public class Food : IData
	{
		// 主键
		[JsonProperty("id")]
		public string Id { get; protected set; }

		// 名称
		[JsonProperty("name")]
		public string Name { get; protected set; }

		// 单价
		[JsonProperty("price")]
		public int Price { get; protected set; }

		// 成本
		[JsonProperty("cost")]
		public int Cost { get; protected set; }

	}

}

using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using ExcelDataReader;
using UnityEngine;

namespace Model.ExcelToCode
{
    /// <summary>
    /// 资源路径定义
    /// </summary>
    public static class ResourcePath
    {
        public static string ConfigPath = $"{Application.dataPath}/Model/ExcelToCode/Config/";// 配置表路径
        public static string DataPath = $"{Application.dataPath}/Model/ExcelToCode/Scripts/Data/";// 生成代码路径
    }

    /// <summary>
    /// 配置表格行号对应数据
    /// </summary>
    public enum ConfigTableRow
    {
        Desc = 0, // 字段描述（注释）
        Name = 1, // 字段名
        Type = 2, // 字段类型
        DataStart = 3, // 数据起始行
    }

    public static class Tools
    {
        /// <summary> 
        /// DataSet转换成Json格式 
        /// </summary> 
        /// <param name="ds">DataSet</param> 
        /// <returns></returns> 
        public static string DataSetToJson(DataSet ds)
        {
            StringBuilder json = new StringBuilder();
            json.Append("{");

            // 遍历table
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                DataTable table = ds.Tables[i];
                object[] name = table.Rows[(int) ConfigTableRow.Name].ItemArray;

                json.Append($"\"{table.TableName}\":[");
                // 遍历行
                for (int row = (int) ConfigTableRow.DataStart; row < table.Rows.Count; row++)
                {
                    json.Append("{");
                    for (int col = 0; col < table.Columns.Count; col++)
                    {
                        json.Append(
                            $"\"{name[col]}\":\"{table.Rows[row][col].ToString().Trim().Replace("\"", "\\\"").Replace("\n", "").Replace("\r", "")}\"");
                        if (col < table.Columns.Count - 1)
                        {
                            json.Append(",");
                        }
                    }

                    json.Append("}");
                    if (row < table.Rows.Count - 1)
                    {
                        json.Append(",");
                    }
                }

                json.Append("]");
                if (i < ds.Tables.Count - 1)
                {
                    json.Append(",");
                }
            }

            json.Append("}");
            return json.ToString();
        }

        /// <summary>
        /// 获取数据表数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DataSet GetExcelDataSet(string path)
        {
            DataSet result;
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    result = reader.AsDataSet();
                }

                stream.Close();
                stream.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 将字符串转换为驼峰命名
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separator">分隔符</param>
        /// <returns></returns>
        public static string ConvertToHumpName(string str, string separator = "_")
        {
            List<string> list = str.Split(separator).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list[i][0].ToString().ToUpper() + list[i].Substring(1);
            }

            // Debug.Log($"字符串：{str}=>{string.Join("", list)}");
            return string.Join("", list);
        }
    }
}
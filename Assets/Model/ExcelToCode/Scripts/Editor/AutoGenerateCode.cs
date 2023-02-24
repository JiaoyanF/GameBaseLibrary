using UnityEditor;
using System.IO;
using System.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace Model.ExcelToCode
{
    /// <summary>
    /// 生成数据表行代码
    /// </summary>
    internal static class AutoGenerateCode
    {
        /// <summary>
        /// 生成代码的命名空间
        /// </summary>
        private static string mCodespace => "Data.Config";
        private static string mDataPath => ResourcePath.ConfigPath;

        [MenuItem("Tools/Generate Config Code", false, 100)]
        private static void HandleAllDataTables()
        {
            //获取指定路径下面的所有资源文件  
            if (Directory.Exists(mDataPath))
            {
                DirectoryInfo direction = new DirectoryInfo(mDataPath);
                FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

                // Debug.Log(files.Length);

                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Name.EndsWith(".meta")) continue;

                    // Debug.Log("Name:" + files[i].Name);
                    // Debug.Log( "FullName:" + files[i].FullName );  
                    // Debug.Log( "DirectoryName:" + files[i].DirectoryName );  
                    LoadFile(files[i].Name, files[i].FullName);
                }
            }
        }

        private static void LoadFile(string fileName, string filePath)
        {
            fileName = fileName.Substring(0, fileName.IndexOf('.')); // 去掉后缀名

            DataSet dataSet = Tools.GetExcelDataSet(filePath);

            StreamWriter sw = InitWriteStream(Tools.ConvertToHumpName(fileName));
            WriteFileStart(sw);
            WriteClass(sw, fileName, dataSet);
            // 遍历sheet
            foreach (DataTable table in dataSet.Tables)
            {
                object[] name = table.Rows[(int) ConfigTableRow.Name].ItemArray;
                object[] type = table.Rows[(int) ConfigTableRow.Type].ItemArray;
                object[] desc = table.Rows[(int) ConfigTableRow.Desc].ItemArray;

                // Debug.Log($"表：{fileName}-{table.TableName}");
                // Debug.Log($"字段名：{JsonConvert.SerializeObject(name)}");
                // Debug.Log($"字段类型：{JsonConvert.SerializeObject(type)}");

                WriteAllProperty(sw, table.TableName, name, type, desc);
            }

            WriteFileEnd(sw);
        }

        private static StreamWriter InitWriteStream(string fileName)
        {
            string path = $"{ResourcePath.DataPath}{fileName}Config.cs";
            if (!File.Exists(path))
            {
                Debug.Log($"创建{fileName}配置映射");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.Create(path).Close();
            }
            else
            {
                Debug.Log($"更新{fileName}配置映射");
            }

            return new FileInfo(path).CreateText();
        }

        private static void WriteFileStart(StreamWriter sw)
        {
            sw.Write("");

            sw.WriteLine("// =====================================================");
            sw.WriteLine("// =");
            sw.WriteLine("// =   该文件为工具生成，如有修改会被覆盖");
            sw.WriteLine("// =");
            sw.WriteLine("// =====================================================");
            sw.WriteLine("using System;");
            sw.WriteLine("using System.Collections.Generic;");
            sw.WriteLine("using Newtonsoft.Json;");
            sw.WriteLine("using UnityEngine;");
            sw.WriteLine("");
            sw.WriteLine($"namespace {mCodespace}");
            sw.WriteLine("{");
        }

        private static void WriteFileEnd(StreamWriter sw)
        {
            sw.WriteLine("}");
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }

        private static void WriteClass(StreamWriter sw, string fileName, DataSet dataSet)
        {
            // 写入配置类
            sw.WriteLine($"\tpublic static class {Tools.ConvertToHumpName(fileName)}Config");
            sw.WriteLine("\t{");
            sw.WriteLine($"\t\tpublic static string FileName => \"{fileName}\";");
            sw.WriteLine("");
            sw.WriteLine($"\t\tprivate static {Tools.ConvertToHumpName(fileName)} data;");
            sw.WriteLine("");
            sw.WriteLine($"\t\tpublic static bool InitData(string json)");
            sw.WriteLine("\t\t{");
            sw.WriteLine("\t\t\ttry");
            sw.WriteLine("\t\t\t{");
            sw.WriteLine($"\t\t\t\tdata = JsonConvert.DeserializeObject<{Tools.ConvertToHumpName(fileName)}>(json);");
            sw.WriteLine("\t\t\t}");
            sw.WriteLine("\t\t\tcatch (Exception e)");
            sw.WriteLine("\t\t\t{");
            sw.WriteLine("\t\t\t\tDebug.Log($\"配置解析失败：{e}\");");
            sw.WriteLine("\t\t\t\treturn false;");
            sw.WriteLine("\t\t\t}");
            sw.WriteLine("");
            sw.WriteLine("\t\t\treturn true;");
            sw.WriteLine("\t\t}");
            sw.WriteLine("");
            foreach (DataTable table in dataSet.Tables)
            {
                string tableName = Tools.ConvertToHumpName(table.TableName);
                sw.WriteLine($"\t\tpublic static List<{tableName}> {tableName}List => data.{tableName}List;");
                sw.WriteLine("");
            }

            sw.WriteLine("\t}");
            sw.WriteLine("");

            // 写入配置类表映射
            sw.WriteLine($"\tpublic class {Tools.ConvertToHumpName(fileName)} : IData");
            sw.WriteLine("\t{");
            foreach (DataTable table in dataSet.Tables)
            {
                string tableName = Tools.ConvertToHumpName(table.TableName);
                sw.WriteLine($"\t\t[JsonProperty(\"{table.TableName}\")]");
                sw.WriteLine($"\t\tpublic readonly List<{tableName}> {tableName}List;");
                sw.WriteLine("");
            }

            sw.WriteLine("\t}");
            sw.WriteLine("");
        }

        private static void WriteAllProperty(StreamWriter sw, string className, object[] names, object[] types,
            object[] descs)
        {
            // 类起始
            sw.WriteLine($"\tpublic class {Tools.ConvertToHumpName(className)} : IData");
            sw.WriteLine("\t{");

            // 遍历写入属性
            for (int i = 0; i < names.Length; i++)
            {
                WriteProperty(sw, names[i].ToString(), types[i].ToString(), descs[i].ToString());
            }

            // 类结束
            sw.WriteLine("\t}");
            sw.WriteLine("");
        }

        private static void WriteProperty(StreamWriter sw, string name, string type, string desc = null)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type)) return;

            if (desc != null)
            {
                sw.WriteLine($"\t\t// {desc}");
            }

            sw.WriteLine($"\t\t[JsonProperty(\"{name}\")]");
            sw.WriteLine($"\t\tpublic {type} {Tools.ConvertToHumpName(name)} {{ get; protected set; }}");
            sw.WriteLine("");
        }
    }
}
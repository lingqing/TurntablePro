using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
 

namespace CoalMonitor.classlib
{
    public class FileXmlOperate
    {
        public static object ReadXmlSerializer(string pathsd, object type)
        {
            XmlSerializer ser = new XmlSerializer(type.GetType());
            System.IO.FileStream fs = new System.IO.FileStream(pathsd, FileMode.Open);
            XmlTextReader reader = new XmlTextReader(fs);
            object ssd = ser.Deserialize(reader);
            fs.Close();
            fs.Dispose();
            return ssd;
        }
        public static bool WriteXmelSerilalzizer(string pathsd, object ooo)
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(ooo.GetType());
                TextWriter writer = new StreamWriter(pathsd);
                ser.Serialize(writer, ooo);
                writer.Close();
                writer.Dispose();
                return true;
            }
            catch (Exception ee)
            {
                return false;
            }
        }
        public static string GetFileNames(string filename)
        {
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filename);// 没有扩展名的文件名 “Default”
            return fileNameWithoutExtension;
        }
        public static bool Exist(string p)
        {

            if (File.Exists(p))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void  CreatPath(string p)
        {
            if (!File.Exists(p))
            {
                File.Create(p);
            }
            
        }
        public static bool ExistFolderPath(string Folderpath)
        {
            return Directory.Exists(Folderpath);
        }
        public static void CreateFoldPath(string Folderpath)
        {
            Directory.CreateDirectory(Folderpath);
        }
        public static string[] getAllMovent(string folderpath, string fileext)
        {
            DirectoryInfo thF = new DirectoryInfo(folderpath);

            List<string> pth = new List<string>();

            FileInfo[] finfo = thF.GetFiles();
            foreach (FileInfo kk in finfo)
            {
                if (kk.Extension == fileext)
                {
                    pth.Add(kk.FullName);
                }

            }
            return pth.ToArray();

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data.SqlClient;

namespace Shared
{
    class Utils
    {
        public string FileName { get; set; }
        public string TempFolder { get; set; }
        public int MaxFileSizeMB { get; set; }
        public List<String> FileParts { get; set; }

        public Utils()
        {
            FileParts = new List<string>();
        }
        public bool MergeFile(string FileName)
        {
            
            
            bool rslt = false;
            string partToken = ".part_";
            string baseFileName = FileName.Substring(0, FileName.IndexOf(partToken));
            string trailingTokens = FileName.Substring(FileName.IndexOf(partToken) + partToken.Length);
            int FileIndex = 0;
            int FileCount = 0;
            int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out FileIndex);
            int.TryParse(trailingTokens.Substring(trailingTokens.IndexOf(".") + 1), out FileCount);
            string Searchpattern = Path.GetFileName(baseFileName) + partToken + "%";

            try
            {
                
                var CS = System.Configuration.ConfigurationManager.AppSettings["DBConnectionString"];

                var FilesList = new List<string>();
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = CS;
                    connection.Open();

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = connection;
                    cmd.CommandTimeout = 0;

                    string commandText = "select f_name from t_files_temp where f_name like '" + Searchpattern + "'";
                    cmd.CommandText = commandText;
                    cmd.CommandType = System.Data.CommandType.Text;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            FilesList.Add(reader.GetString(0));                        
                    }

                    connection.Close();
                }

                //string[] FilesList = Directory.GetFiles(Path.GetDirectoryName(FileName), Searchpattern);
                if (FilesList.Count== FileCount)
                {
                    if (!MergeFileManager.Instance.InUse(baseFileName))
                    {
                        MergeFileManager.Instance.AddFile(baseFileName);
                        
                        
                        List<SortedFile> MergeList = new List<SortedFile>();
                        foreach (string File in FilesList)
                        {
                            SortedFile sFile = new SortedFile();
                            sFile.FileName = File;
                            baseFileName = File.Substring(0, File.IndexOf(partToken));
                            trailingTokens = File.Substring(File.IndexOf(partToken) + partToken.Length);
                            int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out FileIndex);
                            sFile.FileOrder = FileIndex;
                            MergeList.Add(sFile);
                        }
                        var MergeOrder = MergeList.OrderBy(s => s.FileOrder).ToList();

                        using (SqlConnection connection = new SqlConnection())
                        {
                            connection.ConnectionString = CS;
                            connection.Open();

                            SqlCommand cmd = new SqlCommand();
                            cmd.Connection = connection;
                            cmd.CommandTimeout = 0;

                            string commandText = "INSERT INTO t_files VALUES('" +
                                baseFileName + "', (SELECT CAST(f_binarystring AS varchar(MAX)) FROM t_files_temp WHERE f_basename = '" +
                                baseFileName + "' ORDER BY f_id FOR xml PATH('') ) ); ; ";
                            cmd.CommandText = commandText;
                            cmd.CommandType = System.Data.CommandType.Text;

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    FilesList.Add(reader.GetString(0));
                            }

                            connection.Close();
                        }

                        rslt = true;
                        MergeFileManager.Instance.RemoveFile(baseFileName);
                        WebApplication6.Controllers.HomeController.UploadComplete(baseFileName);
                    }
                }
                return rslt;
            }
            catch (Exception ex)
            {

            }

            return rslt;
        }


    }

    public struct SortedFile
    {
        public int FileOrder { get; set; }
        public String FileName { get; set; }
    }

    public class MergeFileManager
    {
        private static MergeFileManager instance;
        private List<string> MergeFileList;

        private MergeFileManager()
        {
            try
            {
                MergeFileList = new List<string>();
            }
            catch { }
        }

        public static MergeFileManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new MergeFileManager();
                return instance;
            }
        }

        public void AddFile(string BaseFileName)
        {
            MergeFileList.Add(BaseFileName);
        }

        public bool InUse(string BaseFileName)
        {
            return MergeFileList.Contains(BaseFileName);
        }

        public bool RemoveFile(string BaseFileName)
        {
            return MergeFileList.Remove(BaseFileName);
        }
    }

}




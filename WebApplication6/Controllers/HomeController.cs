using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.IO;
using System.Data.SqlClient;

namespace WebApplication6.Controllers
{
    public class HomeController : Controller
    {
        int id = 0;
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        void WriteFileToDB(string filename, string binaryString)
        {
            string partToken = ".part_";
            string baseFileName = filename.Substring(0, filename.IndexOf(partToken));
            string trailingTokens = filename.Substring(filename.IndexOf(partToken) + partToken.Length);
            int FileIndex = 0;            
            int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out FileIndex);            
            

            var CS = System.Configuration.ConfigurationManager.AppSettings["DBConnectionString"];

            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = CS;
                connection.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = connection;
                cmd.CommandTimeout = 0;

                string commandText = "delete from t_files_temp where f_name = '" +
                    filename + "'; INSERT INTO t_files_temp VALUES(@Name, @BinaryString, @Id, @BaseName)";
                cmd.CommandText = commandText;
                cmd.CommandType = System.Data.CommandType.Text;
                
                cmd.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 255);
                cmd.Parameters.Add("@BinaryString", System.Data.SqlDbType.VarChar);
                cmd.Parameters.Add("@Id", System.Data.SqlDbType.Int);
                cmd.Parameters.Add("@BaseName", System.Data.SqlDbType.NVarChar, 255);

                cmd.Parameters["@Name"].Value = filename;
                cmd.Parameters["@BinaryString"].Value = binaryString;
                cmd.Parameters["@Id"].Value = FileIndex;
                cmd.Parameters["@BaseName"].Value = baseFileName;

                cmd.ExecuteNonQuery();   

                connection.Close();
            }
        }


        [HttpPost]
        public HttpResponseMessage UploadFile()
        {
            foreach (string file in Request.Files)
            {
                var FileDataContent = Request.Files[file];
                if (FileDataContent != null && FileDataContent.ContentLength > 0)
                {
                    var stream = FileDataContent.InputStream;
                    var fileName = Path.GetFileName(FileDataContent.FileName);
                    
                    try
                    {
                        System.IO.BinaryReader br = new System.IO.BinaryReader(stream);
                        Byte[] bytes = br.ReadBytes((Int32)stream.Length);
                        string base64String = Convert.ToBase64String(bytes, 0, bytes.Length);

                        WriteFileToDB(fileName, base64String);

                        Shared.Utils UT = new Shared.Utils();
                        UT.MergeFile(fileName);
                        
                    }
                    catch (Exception ex)
                    {
                        // handle
                    }
                }
            }
            return new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("File uploaded.")
            };
        }

        public static void UploadComplete(string filename)
        {
            // HERE you can do your own work with a string named binaryString
            
            return;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIADemo.net.bing.api;
using System.IO;
using System.Xml;
using System.Net;
using System.Drawing;
using System.Windows.Forms;

namespace AIADemo
{
    class ImgDB
    {
        const string AppId = "BCFA087B8EFB18381508C1D050378D415D9DE91E";

        private string pathVocabulary;
        private string pathImgDB;
        private int NumPerQuery;
        private Label label3;

        public ImgDB(string pathVocabulary, string pathImgDB, int NumPerQuery, Label label3)
        {
            this.pathVocabulary = pathVocabulary;
            this.pathImgDB = pathImgDB;
            this.NumPerQuery = NumPerQuery;
            this.label3 = label3;
        }

        public bool getImgDB()
        {

            using (BingService service = new BingService())
            {
                StreamReader reader = null;
                SearchRequest request = null;
                SearchResponse response = null;

                try
                {
                    reader = new StreamReader(pathVocabulary, System.Text.Encoding.GetEncoding("gb2312"));
                    
                    for (string query = reader.ReadLine(); query != null; query = reader.ReadLine())
                    {
                        label3.Text = "downloading:" + query + "...";
                        request = BuildRequest(0, NumPerQuery, query);
                        response = service.Search(request);
                        downloadImgs(response, query);
                    }
                    label3.Text = "下载完毕";
                    reader.Close();

                    return true;
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                {
                    // A SOAP Exception was thrown. Display error details.
                    DisplayErrors(ex.Detail);
                }
                catch (System.Net.WebException ex)
                {
                    // An exception occurred while accessing the network.
                    Console.WriteLine(ex.Message);
                }
                catch (System.IO.IOException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
                return false;
            }
        }

        static SearchRequest BuildRequest(int ioffset, int icount, string squery)
        {
            SearchRequest request = new SearchRequest();

            // Common request fields (required)
            request.AppId = AppId;
            request.Query = squery;
            request.Sources = new SourceType[] { SourceType.Image };

            // Common request fields (optional)
            request.Version = "2.0";
            request.Market = "en-us";
            request.Adult = AdultOption.Moderate;
            request.AdultSpecified = true;

            // Image-specific request fields (optional)
            request.Image = new ImageRequest();
            request.Image.Count = (uint)icount;
            request.Image.CountSpecified = true;
            request.Image.Offset = (uint)ioffset;
            request.Image.OffsetSpecified = true;

            return request;
        }

        private void downloadImgs(SearchResponse response, string squery)
        {
            StreamWriter writer = null;
            string pathDir = pathImgDB + "\\" + squery;
            Image img = null;

            try
            {
                Directory.CreateDirectory(pathDir);
                writer = new StreamWriter(pathDir + "\\urls.txt");
                DisplayResponse(response, writer);
                writer.Close();

                // Download the images
                int i = 0;
                foreach (ImageResult result in response.Image.Results)
                {
                    i++;
                    img = getPhoto(result.Thumbnail.Url);
                    if (img != null)
                        img.Save(pathDir + "\\" + i + ".jpg");
                }
            }
            catch (Exception ex)
            {
            }
        }

        private Image getPhoto(string url)
        {
            Image im = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(url);
                request.Method = "GET";
                request.Timeout = 15000; //15000
                request.ProtocolVersion = HttpVersion.Version11;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        im = Image.FromStream(responseStream);
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(url);
                    request.Method = "GET";
                    request.Timeout = 15000; //15000
                    request.ProtocolVersion = HttpVersion.Version11;

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            im = Image.FromStream(responseStream);
                        }
                    }
                }
                catch (Exception exx)
                {
                    return im;
                }
            }
            return im;
        }

        static void DisplayResponse(SearchResponse response, StreamWriter writer)
        {
            // Display the results header.
            Console.WriteLine("Bing API Version " + response.Version);
            Console.WriteLine("Image results for " + response.Query.SearchTerms);
            Console.WriteLine(
                "Displaying {0} to {1} of {2} results",
                response.Image.Offset + 1,
                response.Image.Offset + response.Image.Results.Length,
                response.Image.Total);
            Console.WriteLine();

            // Display the Image results.
            foreach (ImageResult result in response.Image.Results)
            {
                writer.WriteLine(result.MediaUrl);
                writer.WriteLine(result.Title);
                writer.WriteLine(result.Url);
                writer.WriteLine(result.Width);
                writer.WriteLine(result.Height);
                writer.WriteLine(result.Thumbnail.Url);
                writer.WriteLine();
                /*     Console.WriteLine(++i);
                     Console.WriteLine(result.MediaUrl);
                     Console.WriteLine("Page Title: " + result.Title);
                     Console.WriteLine("Page URL: " + result.Url);
                     Console.WriteLine(
                         "Dimensions: "
                         + result.Width
                         + "x"
                         + result.Height);
                     Console.WriteLine("Thumbnail URL: " + result.Thumbnail.Url);
                     Console.WriteLine();
                 */
            }
        }

        static void DisplayErrors(XmlNode errorDetails)
        {
            // Add the default namespace to the namespace manager.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(
                errorDetails.OwnerDocument.NameTable);
            nsmgr.AddNamespace(
                "api",
                "http://schemas.microsoft.com/LiveSearch/2008/03/Search");

            XmlNodeList errors = errorDetails.SelectNodes(
                "./api:Errors/api:Error",
                nsmgr);

            if (errors != null)
            {
                // Iterate over the list of errors and display error details.
                Console.WriteLine("Errors:");
                Console.WriteLine();
                foreach (XmlNode error in errors)
                {
                    foreach (XmlNode detail in error.ChildNodes)
                    {
                        Console.WriteLine(detail.Name + ": " + detail.InnerText);
                    }

                    Console.WriteLine();
                }
            }
        }
    }
}

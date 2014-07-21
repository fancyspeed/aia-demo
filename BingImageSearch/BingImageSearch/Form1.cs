using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

using BingImageSearch.net.bing.api;

namespace BingImageSearch
{
    public partial class Form1 : Form
    {
        const string AppId = "BCFA087B8EFB18381508C1D050378D415D9DE91E";
        const string wordsfilename = "Z:\\city.txt";
        const string outputpath = "Z:\\";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (BingService service = new BingService())
            {
                StreamReader reader = null;
                StreamWriter writer = null;
                SearchRequest request = null;
                SearchResponse response = null;
                try
                {
                    int i;
                    writer = new StreamWriter(outputpath + "AMSTERDAM The Netherlands.txt");
                    for (i = 0; i <= 150; i += 50)
                    {
                        request = BuildRequest(i, 50, "AMSTERDAM The Netherlands");

                        // Send the request; display the response.
                        response = service.Search(request);
                        DisplayResponse(response, writer);
                    }
                    writer.Close();
              /*      reader = new StreamReader(wordsfilename);
                    int i = 0,j = 0;
                    for (string query = reader.ReadLine(); query != null; query = reader.ReadLine())
                    {
                        j++;
                        Console.WriteLine(query);
                        if (j == 12)
                        {
                            Console.WriteLine(query + ":" + j);
                            writer = new StreamWriter(outputpath + j + ".txt");
                            for (i = 0; i <= 150; i += 50)
                            {
                                request = BuildRequest(i, 50, query);

                                // Send the request; display the response.
                                response = service.Search(request);
                                DisplayResponse(response, writer);
                            }
                            writer.Close();
                        }
                    }
                    reader.Close();
               */
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
            
        //    int i = 0;
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
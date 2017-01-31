using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml.Linq;

namespace WebApplication1.Models
{
    public class HttpAdapter
    {
        static string server = "http://localhost:8080/";
        static string apiToken = "9b2d406a6c019fe2a4cdff12da0814b3";
        static string username = "g.madanagopal";
        static string responseFromServer = string.Empty;

        public string TriggerBuild(string projectToken)
        {
            string response = JenkinsTriggerBuild("http://localhost:8080/job/JenkinsApp/build?token=" + projectToken, projectToken);
            return response;
        }

        public static string JenkinsTriggerBuild(string url, string projectToken)
        {
            string nextBuildNumber = GetNextBuildNumber(projectToken);

            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Method = WebRequestMethods.Http.Get;
            byte[] credentialBuffer = new UTF8Encoding().GetBytes(username + ":" + apiToken);

            httpWebRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(credentialBuffer);
            httpWebRequest.PreAuthenticate = true;

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                var isbuilding = WaitUntilBuildFinish(projectToken, nextBuildNumber);
                if (!isbuilding)
                    responseFromServer = GetBuildResult(projectToken, nextBuildNumber);
            }
            catch (WebException excp)
            {
                using (var streamReader = new StreamReader(excp.Response.GetResponseStream()))
                {
                    responseFromServer = streamReader.ReadToEnd();
                }
            }
            return responseFromServer;
        }

        public static bool IsProjectBuilding(string projectToken, string nextBuildNumber)
        {
            //http://localhost:8080/job/JenkinsApp/29/api/xml

            bool isBuilding = false;
            string apiUrl = server + "job/" + projectToken + "/" + nextBuildNumber + "/api/xml";
            string isBuildingStr = GetXmlNodeValue(apiUrl, "building");
            isBuilding = bool.Parse(isBuildingStr);
            return isBuilding;
        }

        public static string GetXmlNodeValue(string apiUrl, string xmlNodeName)
        {
            IEnumerable<XElement> foundElemenets = GetXMLNodeValue(apiUrl, xmlNodeName);
            if (foundElemenets.Count() == 0)
            {
                throw new Exception(
                string.Format("No elements were found for node {0}", xmlNodeName));
            }
            string elementValue = foundElemenets.First().Value;
            return elementValue;
        }

        public static string GetBuildResult(string projectToken, string nextBuildNumber)
        {
            string buildResult = string.Empty;
            try
            {
                string apiUrl = server + "job/" + projectToken + "/" + nextBuildNumber + "/api/xml";
                buildResult = GetXmlNodeValue(apiUrl, "result");
            }
            catch (Exception)
            {

                throw;
            }

            return buildResult;
        }

        public static string GetNextBuildNumber(string jobName)
        {
            string nextBuildNumber = string.Empty;
            try
            {
                string apiUrl = server + "job/" + jobName + "/api/xml";
                nextBuildNumber = GetXmlNodeValue(apiUrl, "nextBuildNumber");
            }
            catch (Exception)
            {

                throw;
            }

            return nextBuildNumber;
        }

        public static IEnumerable<XElement> GetXMLNodeValue(string apiUrl, string xmlNodeName)
        {
            IEnumerable<XElement> foundElemenets;

            try
            {
                var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(apiUrl);
                httpWebRequest.ContentType = "application/xml";
                httpWebRequest.Method = WebRequestMethods.Http.Get;
                byte[] credentialBuffer = new UTF8Encoding().GetBytes(username + ":" + apiToken);

                httpWebRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(credentialBuffer);
                httpWebRequest.PreAuthenticate = true;

                if (httpWebRequest != null)
                {
                    using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            XDocument document = XDocument.Load(stream);
                            XElement root = document.Root;
                            foundElemenets = from element in root.Descendants(xmlNodeName) select element;
                        }
                    }
                }
                else
                    foundElemenets = null;
            }
            catch (Exception)
            {
                throw;
            }
            return foundElemenets;
        }

        public static bool WaitUntilBuildFinish(string projectToken, string nextBuildNumber)
        {
            bool shouldContinue = false;
            string buildStatus = string.Empty;

            //Wait for build creation
            Thread.Sleep(10000);

            do
            {
                bool isProjectBuilding = IsProjectBuilding(projectToken, nextBuildNumber);
                if (isProjectBuilding)
                {
                    shouldContinue = true;
                    Debug.WriteLine("Waits 5 seconds before the new check if the build is completed...");
                    Thread.Sleep(5000);
                }
                else
                {
                    shouldContinue = false;
                }
            }
            while (shouldContinue);

            return shouldContinue;
        }

    }
}
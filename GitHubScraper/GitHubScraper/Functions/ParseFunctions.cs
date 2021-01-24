using GitHubScraper.Model;
using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GitHubScraper.Functions
{
    public class ParseFunctions
    {
        
        public int ParseLinesInt(string numberString)
        {
            int number = 0;
            Int32.TryParse(numberString, out number);
            return number;
        }

        public Double ParseSizeDouble(string numberString)
        {
            numberString = numberString.Replace("Bytes", "");
            bool isKilobyte = false;
            if (numberString.Contains("KB"))
            {
                isKilobyte = true;
                string newString = Regex.Replace(numberString, "[^.0-9]", "");
                numberString = newString;
            }

            Double number = 0;
            Double.TryParse(numberString, out number);
            if (isKilobyte)
            {
                // 1 kb = 1024 bytes
                return number * 1024;
            }
            return number;
        }
        // Obtain code's page and size and group.
        // Creates one object for each extension type
        public bool GetNodePagesAndSize(string innerText, string currentCodeType, List<CodePage> extensionTypeList)
        {
            int currentPageLines = 0;
            double currentPageSize = 0;
            string numberLinesString = "";
            string numberBytesString = "";
            bool hasSize = false;

            if (innerText.Contains("lines"))
            {
                numberLinesString = innerText.Split("lines")[0].Trim();
                // Getting Total Lines result
                if (!String.IsNullOrEmpty(numberLinesString))
                {
                    currentPageLines = ParseLinesInt(numberLinesString);
                }
            }
            if (innerText.Contains("sloc"))
            {
                numberBytesString = innerText.Split("sloc)")[1].Trim();
                if (!String.IsNullOrEmpty(numberBytesString))
                {
                    // Getting Lines value
                    currentPageSize = ParseSizeDouble(numberBytesString);
                    hasSize = true;
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(numberBytesString))
                {
                    currentPageSize = ParseSizeDouble(innerText);
                    hasSize = true;
                }
            }
            bool flagFileTypeExist = false;
            // Adding code bytes and lines to an object with the same code extension 
            foreach (var codePage in extensionTypeList)
            {
                if (codePage.FileExtension.Equals(currentCodeType))
                {
                    codePage.FileExtension = currentCodeType;
                    codePage.TotalBytes += currentPageSize;
                    codePage.TotalLines += currentPageLines;
                    codePage.Quantity += 1;
                    flagFileTypeExist = true;
                    break;
                }
            }
            // Creates a new object for the current extension type if it does not exist
            if ((extensionTypeList.Count == 0 || !flagFileTypeExist) && !(currentCodeType.Equals("xunittest")))
            {
                CodePage newCodePageType = new CodePage();
                newCodePageType.FileExtension = currentCodeType;
                newCodePageType.TotalBytes = currentPageSize;
                newCodePageType.TotalLines = currentPageLines;
                newCodePageType.Quantity = 1;
                extensionTypeList.Add(newCodePageType);
            }
            return hasSize;
        }

        // Get HTML nodes by element class and URL and returns a list its files url's
        public void GetNodes(string url, string elementClass, ConcurrentBag<string> list, List<CodePage> extensionTypeList)
        {
            try
            {
                //Obtain pages html code
                var web = new HtmlWeb();
                var doc = web.Load(url);

                string finalPathClass = "final-path";
                string codeLinesClass = "text-mono f6 flex-auto pr-3 flex-order-2 flex-md-order-1 mt-2 mt-md-0";
                string currentCodeType = "";

                // Check if current page is code by trying to find a node with the element class where is written its lines and size
                var codePageNode = doc.DocumentNode.SelectNodes($"//*[@class='{codeLinesClass}']");
                if ((codePageNode != null) && codePageNode.Count > 0)
                {
                    // Getting final path node in order to get code type
                    var finalPathNode = doc.DocumentNode.SelectNodes($"//*[@class='{finalPathClass}']");
                    if ((finalPathNode != null) && finalPathNode.Count > 0)
                    {
                        // Getting code extension
                        string[] codeNameTag = finalPathNode[0].OuterHtml.Split(".");
                        int codeTypeIndex = finalPathNode[0].OuterHtml.Split(".").Length - 1;
                        currentCodeType = codeNameTag[codeTypeIndex].Replace("</strong>", "");
                        GetNodePagesAndSize(codePageNode[0].InnerText.Trim(), currentCodeType, extensionTypeList);
                    }
                }
                // If  its not a code, its probably a folder
                else
                {
                    // Inserting page files URLs into our list to be checked for codes
                    var nodes = doc.DocumentNode.SelectNodes($"//*[@class='{elementClass}']");
                    if (nodes != null)
                    {
                        foreach (var fileName in nodes)
                        {
                            list.Add("https://github.com/" + fileName.OuterHtml.Split("\"")[5]);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

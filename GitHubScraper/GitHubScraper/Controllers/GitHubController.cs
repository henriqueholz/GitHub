using GitHubScraper.Functions;
using GitHubScraper.Model;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GitHubScrapper.Controllers
{
    // Request example: http://localhost:51983/github/henriqueholz/GitHubScraper/
    [ApiController]
    [Route("[controller]")]
    public class GitHubController : ControllerBase
    {
        List<CodePage> extensionTypeList = new List<CodePage>();
        [HttpGet("{account}/{repository}")]
        public string GetGitHub(string account, string repository)
        {
            //Get the content of the URL from the Web
            string repositoryUrl = $"https://github.com/{account}/{repository}";

            const string filesElementClass = "js-navigation-open link-gray-dark";

            List<ConcurrentBag<string>> urlPathList = new List<ConcurrentBag<string>>();
            ConcurrentBag<string> urlPath = new ConcurrentBag<string>();
            urlPath.Add(repositoryUrl);
            urlPathList.Add(urlPath);
            
            int i = 0;  
            bool exitWhile = false;

            ParseFunctions parse = new ParseFunctions();
            while (!exitWhile)
            {
                urlPathList.Add(new ConcurrentBag<string>());
                Parallel.ForEach(urlPathList[i], item =>
                {
                    // Get current parsing page folder's files and stores it in a new list
                    parse.GetNodes(item, filesElementClass, urlPathList[i + 1], extensionTypeList);
                });
                if (urlPathList[i + 1].Count == 0)
                {
                    // There is no more files to parse
                    exitWhile = true;
                }
                i++;
            }
            string jsonString = JsonSerializer.Serialize(extensionTypeList);
            return jsonString;
        }

    }
}
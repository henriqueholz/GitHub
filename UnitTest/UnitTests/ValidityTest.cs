using GitHubScraper.Functions;
using GitHubScraper.Model;
using GitHubScrapper.Controllers;
using System;
using System.Collections.Generic;
using Xunit;

namespace XUnitTestProject
{
    public class ValidityTest
    {
        [Fact]
        public void HeaderWithLinesAndSize()
        {
            //Arrange
            string headerNodeInnertext = @"

                23 lines(23 sloc)


              470 Bytes
            ";

            ParseFunctions parseFunctions = new ParseFunctions(); 

            //Act
            bool isValid = parseFunctions.GetNodePagesAndSize(headerNodeInnertext, "xunittest", new List<CodePage>());

            //Assert
            Assert.True(isValid);
        }

    }
}

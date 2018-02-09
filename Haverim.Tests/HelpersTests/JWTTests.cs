using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Haverim.Tests.HelpersTests
{
    [TestClass]
    public class JWTTests
    {
        [TestMethod]
        public void GetTokenTest()
        {
            var payload = new Controllers.Helpers.ApiClasses.Payload
            {
                Username = "Some Username"
            };
            string token = Controllers.Helpers.JWT.GetToken(payload);

            Assert.AreEqual(payload, Controllers.Helpers.JWT.VerifyToken(token).Item2);
            token += "_";
            Assert.AreEqual(Controllers.Helpers.JWT.VerifyToken(token).Item1, Controllers.Helpers.JWT.TokenStatus.InvalidSignature);
        }
    }
}

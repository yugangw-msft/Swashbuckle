﻿using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Swashbuckle.Application;
using Swashbuckle.Dummy.Controllers;
using System.Web.Http;
using System.Linq;

namespace Swashbuckle.Tests.SwaggerSpec
{
    [TestFixture]
    public class MultipleApiVersionsTests : HttpMessageHandlerTestsBase<SwaggerSpecHandler>
    {
        private SwaggerSpecConfig _swaggerSpecConfig;

        public MultipleApiVersionsTests()
            : base("swagger/{apiVersion}/api-docs")
        { }

        [SetUp]
        public void SetUp()
        {
            _swaggerSpecConfig = new SwaggerSpecConfig();
            Handler = new SwaggerSpecHandler(_swaggerSpecConfig);

            SetUpDefaultRoutesFor(new[] { typeof(ProductsController), typeof(CustomersController) });
        }

        [Test]
        public void It_should_support_multiple_api_versions_by_provided_strategy()
        {
            _swaggerSpecConfig.SupportMultipleApiVersions(
                new string[] { "1.0", "2.0" },
                (apiDesc, version) =>
                {
                    var controllerName = apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName;
                    return (version == "1.0" && controllerName == "Products")
                        || (version == "2.0" && new[] { "Products", "Customers" }.Contains(controllerName));
                });

            var v1Listing = Get<JObject>("http://tempuri.org/swagger/1.0/api-docs");
            var expected = JObject.FromObject(
                new
                {
                    swaggerVersion = "1.2",
                    apis = new object[]
                    {
                        new { path = "/Products" }
                    },
                    apiVersion = "1.0"
                });
            Assert.AreEqual(expected.ToString(), v1Listing.ToString());

            var v2Listing = Get<JObject>("http://tempuri.org/swagger/2.0/api-docs");
            expected = JObject.FromObject(
                new
                {
                    swaggerVersion = "1.2",
                    apis = new object[]
                    {
                        new { path = "/Customers" },
                        new { path = "/Products" }
                    },
                    apiVersion = "2.0"
                });
            Assert.AreEqual(expected.ToString(), v2Listing.ToString());
        }

   }
}

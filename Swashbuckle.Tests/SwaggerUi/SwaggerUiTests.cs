﻿using System;
using System.Net.Http;
using System.Net;
using NUnit.Framework;
using Swashbuckle.Application;
using Swashbuckle.Dummy;
using Swashbuckle.WebAssets;

namespace Swashbuckle.Tests.SwaggerUi
{
    [TestFixture]
    public class SwaggerUiTests : HttpMessageHandlerTestBase<SwaggerUiHandler>
    {
        public SwaggerUiTests()
            : base("swagger/ui/{*uiPath}")
        { }

        [SetUp]
        public void SetUp()
        {
            // Default set-up
            SetUpHandler();
        }

        [Test]
        public void It_serves_the_embedded_swagger_ui()
        {
            var content = GetContentAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains("rootUrl: 'http://tempuri.org:80/'", content);
            StringAssert.Contains("discoveryPaths: [ 'swagger/docs/1.0' ]", content);
            StringAssert.Contains("swagger-ui-container", content);
        }
        
        [Test]
        public void It_exposes_config_to_inject_custom_stylesheets()
        {
            SetUpHandler(c =>
                {
                    var assembly = typeof(SwaggerConfig).Assembly;
                    c.InjectStylesheet(assembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");
                    c.InjectStylesheet(assembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles2.css");
                });


            var content = GetContentAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains(
                "<link href='ext/Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css' media='screen' rel='stylesheet' type='text/css' />\r\n" +
                "<link href='ext/Swashbuckle.Dummy.SwaggerExtensions.testStyles2.css' media='screen' rel='stylesheet' type='text/css' />",
                content);

            content = GetContentAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");
            StringAssert.StartsWith("h1", content);

            content = GetContentAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle.Dummy.SwaggerExtensions.testStyles2.css");
            StringAssert.StartsWith("h2", content);
        }
        
        [Test]
        public void It_exposes_config_for_swagger_ui_settings()
        {
            SetUpHandler(c =>
                {
                    c.SupportHeaderParams();
                    c.SupportedSubmitMethods(new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head });
                    c.DocExpansion(DocExpansion.Full);
                });

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains("supportHeaderParams: true", content);
            StringAssert.Contains("supportedSubmitMethods: [ 'get','post','put','head' ]", content);
            StringAssert.Contains("docExpansion: 'full'", content);
        }
        
        [Test]
        public void It_exposes_config_for_swagger_ui_outh2_settings()
        {
            SetUpHandler(c => c.EnableOAuth2Support("test-client-id", "test-realm", "Swagger UI"));

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains("oAuth2Enabled: true", content);
            StringAssert.Contains("oAuth2ClientId: 'test-client-id'", content);
            StringAssert.Contains("oAuth2Realm: 'test-realm'", content);
            StringAssert.Contains("oAuth2AppName: 'Swagger UI'", content);
        }

        [Test]
        public void It_exposes_config_to_enable_a_discovery_url_selector()
        {
            SetUpHandler(c => c.EnableDiscoveryUrlSelector());

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains("Swashbuckle.SwaggerExtensions.discoveryUrlSelector.js", content);
        }
        

        [Test]
        public void It_exposes_config_to_inject_custom_javascripts()
        {
            SetUpHandler(c =>
                {
                    var assembly = typeof(SwaggerConfig).Assembly;
                    c.InjectJavaScript(assembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript1.js");
                    c.InjectJavaScript(assembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript2.js");
                });

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains(
                "customScripts: [ " +
                "'ext/Swashbuckle.Dummy.SwaggerExtensions.testScript1.js'," +
                "'ext/Swashbuckle.Dummy.SwaggerExtensions.testScript2.js' " +
                "]",
                content);

            content = GetContentAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle.Dummy.SwaggerExtensions.testScript1.js");
            StringAssert.StartsWith("var str1", content);

            content = GetContentAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle.Dummy.SwaggerExtensions.testScript2.js");
            StringAssert.StartsWith("var str2", content);
        }
        
        [Test]
        public void It_exposes_config_to_serve_custom_web_assets()
        {
            SetUpHandler(c =>
                {
                    var assembly = typeof(SwaggerConfig).Assembly;
                    c.CustomWebAsset("index.html", assembly, "Swashbuckle.Dummy.SwaggerExtensions.myIndex.html");
                });

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains("My Index", content);
        }
        
        [Test]
        public void It_errors_on_web_assets_not_found_and_returns_status_not_found()
        {
            var response = Get("http://tempuri.org/swagger/ui/ext/foobar");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        private void SetUpHandler(Action<SwaggerUiConfig> configure = null)
        {
            var swaggerUiConfig = new SwaggerUiConfig(new[] { "swagger/docs/1.0" });
            if (configure != null)
                configure(swaggerUiConfig);

            var swaggerUiProvider = new EmbeddedWebAssetProvider(swaggerUiConfig.ToUiProviderSettings());

            Handler = new SwaggerUiHandler(Swashbuckle.Configuration.DefaultRootUrlResolver, swaggerUiProvider);
        }
    }
}
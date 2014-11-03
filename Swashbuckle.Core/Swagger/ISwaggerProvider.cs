﻿using System;

namespace Swashbuckle.Swagger
{
    public interface ISwaggerProvider
    {
        SwaggerDocument GetSwaggerFor(string apiVersion, string apiRootUrl);
    }

    public class UnknownApiVersion : Exception
    {
        public UnknownApiVersion(string apiVersion)
            : base(String.Format("Unknown API version - {0}", apiVersion))
        {}
    }
}
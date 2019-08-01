﻿using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public static class XmlHelper
    {
        public static XDocument Convert(Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (var xr = XmlReader.Create(stream))
            {
                return XDocument.Load(xr);
            }
        }
    }
}
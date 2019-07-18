﻿using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    [XmlRoot("xml")]
    public class ImageResponse : ResponseMessage
    {
        public ImageResponse()
        {
        }

        public ImageResponse(Image image)
        {
            Image = image;
        }

        public ImageResponse(string mediaId)
        {
            Image = new Image(mediaId);
        }

        [XmlIgnore]
        public override string MsgType => ResponseMessageType.Image;

        [XmlElement(ElementName = "MsgType")]
        public System.Xml.XmlCDataSection MsgTypeCDATA
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(MsgType);
            }

            set
            {
                MsgType = value.Value;
            }
        }

        [XmlElement(ElementName = "Image")]
        public Image Image { get; set; }
    }
}

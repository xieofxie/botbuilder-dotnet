﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events.Common
{
    [XmlRoot("xml")]
    public class TemplateSendFinishedEvent : RequestEvent
    {
        public override string Event => EventType.TemplateSendFinished;

        [XmlElement(ElementName = "Status")]
        public string Status { get; set; }

        [XmlElement(ElementName = "MsgID")]
        public long MsgID { get; set; }
    }
}

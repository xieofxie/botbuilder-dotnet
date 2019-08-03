using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.HandoffEx
{
    public interface IHandoffActivity : IActivity
    {
        string Value { get; set; }

        IList<Activity> Activities { get; set; }
    }
}

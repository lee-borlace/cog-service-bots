using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISpyBot
{
    public class ISpyBotState
    {
        public string ObjectChosenByBot { get; set; }

        public int NumberOfGuesses { get; set; }

        public bool WaitingForTagsFromVision { get; set; }

        public ISpyBotState()
        {
            ObjectChosenByBot = string.Empty;
            NumberOfGuesses = 0;
            WaitingForTagsFromVision = false;
        }
    }
}

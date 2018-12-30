using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISpyBot
{
    public class Constants
    {
        public class Messages
        {
            public const string PromptPlayISpy = "Hi, would you like to play I Spy? I'll need to use your camera to play.";
            public const string DontPlay = "No worries, let me know if you'd like to play in future!";
            public const string StartingCamera = "OK, let's start. Give me a second while I see what I can see. If you get prompted to let me use the camera, please select Yes!";
            public const string GotOne = "OK, got one!";
            public const string ISpy = "I spy, with my little eye, something beginning with {0}.";
            public const string CouldntFindAnything = "Hmm, I'm having trouble seeing anything.";
        }

        public class InputHint
        {
            public const string AcceptingInput = "acceptingInput";
            public const string ExpectingInput = "expectingInput";
            public const string IgnoringInput = "ignoringInput";
        }

        public class BotEvents
        {
            public const string ReadyForCamera = "readyForCamera";
            public const string ImageAnalysed = "imageAnalysed";
        }
    }
}

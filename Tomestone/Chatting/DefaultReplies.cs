using System;
using System.Collections.Generic;

namespace Tomestone.Chatting
{
    public static class DefaultReplies
    {
        private static readonly Random Random = new Random();


        private static readonly List<string> AngryMessages = new List<string>
        {
            "Don't you even think about it.",
            "Do that again and I'll have you.",
            "Did you really just..",
            "Don't you dare.",
            "Once more and you're out.",
            "Are you trying to get kicked?",
            "Stop it. Seriously.",
            "Are you seriously asking me to do that?",
            "I can't believe you thought that would be a good idea.",
            "Knock it off!",
            "Get out of here.",
            "Stop annoying me.",
            "That will never happen!",
            "Do you want to be removed?",
            "Cut it out!",
            "Keep up and we are going to have a problem...",
            "Are you trying to pick a fight?",
            "Now you're just asking for trouble.",
        };

        private static readonly List<string> ConfirmationMessages = new List<string>
        {
            "Ok.",
            "Uh, sure.",
            "I can do that.",
            "Will do!",
            "Sure. I guess.",
            "Uhhu.",
            "I'll see what I can do.",
            "Fine..",
            "Hmn. Alright.",
            "Yeah yeah.",
            "/me nods.",
            "/me will do.",
            "/me will try.",
            "It will be done.",
            "That shouldn't be too hard.",
            "Just for you.",
            "I will do it just for you.",
            "Just because you asked...",
            "Consider it done.",
            "You've got a deal.",
            "It's a done deal.",
            "Got that in the bag.",
            "Easy!",
            "No problem.",
            "Done and Done!",
            "As directed.",
            "Of course!",
            "Anything else?",
            "Because you asked nicely~",
        };

        private static readonly List<string> DenialMessages = new List<string>
        {
            "Nope.",
            "No.",
            "Or not.",
            "Won't do.",
            "Let's not, alright?",
            "Afraid I can't do that.",
            "As if.",
            "I won't do that.",
            "Kidding? Nope.",
            "Yeah... no.",
            "/me shakes its head.",
            "/me sighs and denies.",
            "/me won't.",
            "Not an option.",
            "What? No.",
            "I could... but no.",
            "You can't be serious.",
            "Yeah... That ain't happening.",
            "What a harebrained idea.",
        };

        private static List<string> ErrorMessages = new List<string>
        {
            "Whoops, I think I divided that by zero.",
        };

        private static string GetRandom(List<string> messages)
        {
            var arr = messages.ToArray();
            var i = Random.Next(0, arr.Length);
            return arr[i];
        }
        public static string Angry()
        {
            return GetRandom(AngryMessages);
        }

        public static string Confirmation()
        {
            return GetRandom(ConfirmationMessages);
        }

        public static string Denial()
        {
            return GetRandom(DenialMessages);
        }

        public static string Error()
        {
            return GetRandom(ErrorMessages);
        }
    }
}
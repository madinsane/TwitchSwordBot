using System;

namespace TwitchSwordBot {
    public class ChatMessage : EventArgs {
        public string Message { get; set; }
        public string Sender { get; set; }
        public string Channel { get; set; }
        public string Tags { get; set; }
    }
}
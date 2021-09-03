using System;

namespace TwitchSwordBot {
    /// <summary>
    /// Describes a chat message received from IRC
    /// </summary>
    public class ChatMessage : EventArgs {
        public string Message { get; set; }
        public string Sender { get; set; }
        public string Channel { get; set; }
        public string Tags { get; set; }
    }
}
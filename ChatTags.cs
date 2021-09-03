namespace TwitchSwordBot {
    /// <summary>
    /// Describes the chat tags that can be attached to messages received from IRC<br/>
    /// These can be used to check many details about a user or a specific message that would otherwise require extra requests
    /// </summary>
    public class ChatTags {
        /// <summary>
        /// Is user a moderator, staff member or global mod
        /// </summary>
        public bool IsMod { get; set; } = false;
        /// <summary>
        /// Is user a subscriber
        /// </summary>
        public bool IsSub { get; set; } = false;
        /// <summary>
        /// Is user the broadcaster
        /// </summary>
        public bool IsBroadcaster { get; set; } = false;
        /// <summary>
        /// Is user a VIP
        /// </summary>
        public bool IsVIP { get; set; } = false;
        /// <summary>
        /// Message unique id
        /// </summary>
        public string MsgId { get; set; }
        /// <summary>
        /// Unique user id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Checks if user is priviledged status 
        /// </summary>
        /// <returns><c>true</c> if moderator, broacaster or VIP</returns>
        public bool HasPrivilege() {
            return IsMod || IsBroadcaster || IsVIP;
        }
    }
}
namespace TwitchSwordBot {
    public class ChatTags {
        public bool IsMod { get; set; } = false;
        public bool IsSub { get; set; } = false;
        public bool IsBroadcaster { get; set; } = false;
        public bool IsVIP { get; set; } = false;
        public string MsgId { get; set; }
        public string UserId { get; set; }

        public bool HasPrivilege() {
            return IsMod || IsBroadcaster || IsVIP;
        }
    }
}
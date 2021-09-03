using System;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using AsyncAwaitBestPractices;
using System.Linq;
using TwitchSwordBot.Imports;
using HomoglyphConverter;

namespace TwitchSwordBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Init bot
            SwordBot swordBot = new();
            swordBot.Start().SafeFireAndForget();
            swordBot.VerboseConsole = false;
            swordBot.OnMessage += async (sender, chatMessage) =>
            {
                Console.WriteLine($"#{chatMessage.Channel} {chatMessage.Sender}: {chatMessage.Message}");
                ChatTags chatTags = new();
                if (chatMessage.Tags != "") {
                    chatTags = ParseTags(chatMessage.Tags);
                }
                if (chatTags.HasPrivilege() && chatMessage.Channel == swordBot.User) {
                    //Home commands
                    if (chatMessage.Message.StartsWith("!kill")) {
                        await swordBot.Kill();
                        Environment.Exit(0);
                    } else if (chatMessage.Message.StartsWith("!join ")) {
                        string[] split = chatMessage.Message.Split(" ");
                        if (split.Length >= 2) {
                            await swordBot.JoinChannel(split[1]);
                        }
                    } else if (chatMessage.Message.StartsWith("!leave ")) {
                        string[] split = chatMessage.Message.Split(" ");
                        if (split.Length >= 2) {
                            await swordBot.LeaveChannel(split[1]);
                        }
                    } else if (chatMessage.Message.StartsWith("!m ") ||
                        chatMessage.Message.StartsWith("!msg ")) {
                        string[] split = chatMessage.Message.Split(" ");
                        if (split.Length >= 3) {
                            await swordBot.SendMessage(split[1], string.Join(' ', split.Skip(2)));
                        }
                    } else if (chatMessage.Message.StartsWith("!banword ")) {
                        string[] split = chatMessage.Message.Split(" ");
                        if (split.Length >= 2) {
                            swordBot.AddBannedWord(HelperFunctions.GetStringRemainder(chatMessage.Message, "!banword"));
                            await swordBot.SendMessage(chatMessage.Channel, "Banned word");
                        }
                    } else if (chatMessage.Message.StartsWith("!unbanword ")) {
                        string[] split = chatMessage.Message.Split(" ");
                        if (split.Length >= 2) {
                            swordBot.AddBannedWord(HelperFunctions.GetStringRemainder(chatMessage.Message, "!unbanword"));
                            await swordBot.SendMessage(chatMessage.Channel, "Unbanned word");
                        }
                    }
                }
                bool banning = false;
                //Message checks
                //if (!chatTags.HasPrivilege()) {
                    if (swordBot.CheckStringForBannedWords(chatMessage.Message)) {
                        banning = true;
                        await swordBot.SendMessage(chatMessage.Channel, "BAD WORD DETECTED MrDestructoid");
                    }
                //}
                //General commands
                if (!banning) {
                    if (chatMessage.Message.StartsWith("!Hi")) {
                        await swordBot.SendMessage(chatMessage.Channel, $"Hello {chatMessage.Sender}");
                    }
                }
            };
            await Task.Delay(-1);
        }

        private static ChatTags ParseTags(string tags) {
            string tagsTrim = tags.TrimStart('@');
            string[] tagsSplit = tagsTrim.Split(";");
            ChatTags chatTags = new();
            foreach (string tag in tagsSplit) {
                if (tag.StartsWith("badges")) {
                    //Parse badges
                    if (tag.IndexOf("broadcaster") >= 0) {
                        chatTags.IsBroadcaster = true;
                    }
                    if (tag.IndexOf("subscriber") >= 0) {
                        chatTags.IsSub = true;
                    }
                    if (tag.IndexOf("admin") >= 0 ||
                        tag.IndexOf("staff") >= 0 ||
                        tag.IndexOf("global_mod") >= 0 ||
                        tag.IndexOf("moderator") >= 0) {
                        chatTags.IsMod = true;
                    }
                } else if (tag.StartsWith("id")) {
                    chatTags.MsgId = HelperFunctions.GetStringRemainder(tag, "id=");
                } else if (tag.StartsWith("mod")) {
                    chatTags.IsMod = HelperFunctions.GetStringRemainder(tag, "mod=") == "1";
                } else if (tag.StartsWith("subscriber")) {
                    chatTags.IsSub = HelperFunctions.GetStringRemainder(tag, "subscriber=") == "1";
                } else if (tag.StartsWith("user-id")) {
                    chatTags.UserId = HelperFunctions.GetStringRemainder(tag, "user-id=");
                }
            }
            
            return chatTags;
        }
    }
}

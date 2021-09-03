using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TwitchSwordBot {
    /// <summary>
    /// Core class for a twitch bot
    /// </summary>
    class SwordBot {
        const string USER_FILE = "User.txt";
        const string BANNED_WORDS_FILE = "BannedWords.txt";
        internal string Pass { get; set; }
        internal string User { get; set; }
        const string IP = "irc.chat.twitch.tv";
        const int PORT = 6697;
        private TaskCompletionSource<int> connected = new TaskCompletionSource<int>();
        private StreamWriter tcpWriter;
        private StreamReader tcpReader;
        public delegate void ChatEventHandler(object sender, ChatMessage e);
        public event ChatEventHandler OnMessage = delegate { };
        /// <summary>
        /// If <c>true</c> writes all received messages to console
        /// </summary>
        public bool VerboseConsole { get; set; } = false;
        private bool isAlive;
        private HashSet<string> joinedChannels;
        private Dictionary<string, string> bannedWords;

        public SwordBot() {
            joinedChannels = new HashSet<string>();
            bannedWords = new Dictionary<string, string>();
            GetUserInfo();
            GetBannedWords();
            Console.WriteLine("User set to: " + User);
            //Console.WriteLine(Pass);
        }

        /// <summary>
        /// Starts the bot
        /// </summary>
        /// <returns></returns>
        public async Task Start() {
            //Init TCP connection
            TcpClient tcpClient = new();
            await tcpClient.ConnectAsync(IP, PORT);
            SslStream sslStream = new(
                tcpClient.GetStream(),
                false,
                ValidateServerCertificate,
                null
            );
            await sslStream.AuthenticateAsClientAsync(IP);
            tcpReader = new(sslStream);
            tcpWriter = new(sslStream) {
                NewLine = "\r\n",
                AutoFlush = true,
            };
            //Authorise
            await tcpWriter.WriteLineAsync($"PASS {Pass}");
            await tcpWriter.WriteLineAsync($"NICK {User}");
            //Login done
            connected.SetResult(0);
            await tcpWriter.WriteLineAsync($"CAP REQ :twitch.tv/tags");
            await JoinChannel(User);
            await SendMessage(User, "MrDestructoid Beep Boop MrDestructoid");
            isAlive = true;
            while (isAlive) {
                await connected.Task;
                string line = "";
                line = await tcpReader.ReadLineAsync();
                if (VerboseConsole) {
                    Console.WriteLine(line);
                }
                //Handle Tags
                string[] split;
                string tags = "";
                string remLine = line;
                if (string.IsNullOrEmpty(line)) {
                    await Task.Delay(20);
                    continue;
                }
                if (line.StartsWith("@")) {
                    string[] tagSplit = line.Split(" ");
                    tags = tagSplit[0];
                    split = tagSplit.Skip(1).ToArray();
                    remLine = HelperFunctions.GetStringRemainder(line, tags).TrimStart();
                } else {
                    split = line.Split(" ");
                }
                //Handle Ping
                if (remLine.StartsWith("PING")) {
                    Console.WriteLine(split[0]);
                    await tcpWriter.WriteLineAsync($"PONG {split[1]}");
                //Handle Message
                } else if (split.Length > 2 && split[1] == "PRIVMSG") {
                    //Parse Message
                    int exclamPos = split[0].IndexOf("!");
                    string username = split[0].Substring(1, exclamPos - 1);
                    int secondColonPos = remLine.IndexOf(':', 1);
                    string message = remLine.Substring(secondColonPos + 1);
                    string channel = split[2].TrimStart('#');
                    //Event Handler
                    OnMessage(this, new ChatMessage
                    {
                        Message = message,
                        Sender = username,
                        Channel = channel,
                        Tags = tags
                    });
                } else if (split.Length > 2 && split[1] == "JOIN") {
                    string channel = split[2].TrimStart('#');
                    if (channel != User) {
                        Console.WriteLine($"Successfully joined channel: {channel}");
                        await SendMessage(User, $"Successfully joined channel: {channel}");
                    }
                    joinedChannels.Add(channel);
                } else if (split.Length > 2 && split[1] == "PART") {
                    string channel = split[2].TrimStart('#');
                    Console.WriteLine($"Successfully left channel: {channel}");
                    await SendMessage(User, $"Successfully left channel: {channel}");
                    if (joinedChannels.Contains(channel)) {
                        joinedChannels.Remove(channel);
                    }
                }
            }
        }

        private bool GetUserInfo() {
            string userPrefix = "user:";
            string passPrefix = "pass:";
            try {
                string[] file = File.ReadAllLines(USER_FILE);
                foreach (string line in file) {
                    if (line.StartsWith(userPrefix)) {
                        User = HelperFunctions.GetStringRemainder(line, userPrefix).Trim();
                    } else if (line.StartsWith(passPrefix)) {
                        Pass = HelperFunctions.GetStringRemainder(line, passPrefix).Trim();
                    }
                }
                return true;
            } catch(IOException e) {
                Console.WriteLine("Failed to read User file (User.txt):\n" + e.Message);
                System.Environment.Exit(-1);
            }
            return false;
        }

        private void GetBannedWords() {
            try {
                if (File.Exists(BANNED_WORDS_FILE)) {
                    string[] lines = File.ReadAllLines(BANNED_WORDS_FILE);
                    foreach (string line in lines) {
                        string trim = line.Trim();
                        if (!bannedWords.ContainsKey(trim) && !string.IsNullOrWhiteSpace(trim)) {
                            bannedWords.Add(trim.ToLower(),
                                HelperFunctions.ConvertStringToPlain(trim.ToLower()).ToLower());
                        }
                    }
                }
            } catch (IOException e) {
                Console.WriteLine("Failed to read Banned words file (BannedWords.txt):\n" + e.Message);
                System.Environment.Exit(-1);
            }
        }

        /// <summary>
        /// Adds a new banned word/phrase
        /// </summary>
        /// <param name="banWord">Word/phrase to ban</param>
        public void AddBannedWord(string banWord) {
            if (!bannedWords.ContainsKey(banWord)) {
                bannedWords.Add(banWord, HelperFunctions.ConvertStringToPlain(banWord));
            }
            WriteToBannedWords();
        }

        /// <summary>
        /// Removes an existing banned word/phrase
        /// </summary>
        /// <param name="banWord">Word/phrase to remove</param>
        public void RemoveBannedWord(string banWord) {
            if (bannedWords.ContainsKey(banWord)) {
                bannedWords.Remove(banWord);
            }
            WriteToBannedWords();
        }

        private void WriteToBannedWords() {
            File.WriteAllLines(BANNED_WORDS_FILE, bannedWords.Keys);
        }

        /// <summary>
        /// Checks a string for banned words/phrases using homoglyph parsing
        /// </summary>
        /// <param name="str">String to check for banned phrases</param>
        /// <returns><c>true</c> if contains any banned phrases, <c>false</c> otherwise</returns>
        public bool CheckStringForBannedWords(string str) {
            string[] split = str.Split(" ");
            for (int i=0; i<split.Length; i++) {
                if (i+1 < split.Length) {
                    if (CheckIfBannedWord(split[i], split.Skip(i+1).ToArray(), bannedWords)) {
                        return true;
                    }
                } else {
                    if (CheckIfBannedWord(split[i], new string[0], bannedWords)) {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckIfBannedWord(string str, string[] full, Dictionary<string, string> search) {
            string plain = HelperFunctions.ConvertStringToPlain(str);
            string flip = HelperFunctions.FlipStringAndStrip(str);
            foreach (KeyValuePair<string, string> ban in search) {
                if (str.Equals(ban.Key) || plain.Equals(ban.Key) || flip.Equals(ban.Key) ||
                    str.Equals(ban.Value) || plain.Equals(ban.Value) || flip.Equals(ban.Value)) {
                    return true;
                }
            }
            if (full.Length < 1) {
                return false;
            }
            Dictionary<string, string> matches = new();
            foreach (KeyValuePair<string, string> ban in search) {
                if (ban.Key.Contains(str) || ban.Value.Contains(str)) 
                {
                    matches.TryAdd(HelperFunctions.GetStringRemainder(ban.Key, str),
                    HelperFunctions.GetStringRemainder(ban.Value, str));
                } else if (ban.Key.Contains(plain) || ban.Value.Contains(plain)) {
                    matches.TryAdd(HelperFunctions.GetStringRemainder(ban.Key, plain),
                    HelperFunctions.GetStringRemainder(ban.Value, plain));
                } else if (ban.Key.Contains(flip) || ban.Value.Contains(flip)) {
                    matches.TryAdd(HelperFunctions.GetStringRemainder(ban.Key, flip),
                    HelperFunctions.GetStringRemainder(ban.Value, flip));
                }
            }
            if (matches.Count > 0) {
                if (full.Length > 2) {
                    return CheckIfBannedWord(full[0], full.Skip(1).ToArray(), matches);
                } else {
                    return CheckIfBannedWord(full[0], new string[0], matches);
                }
            }
            return false;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }

        /// <summary>
        /// Sends a message to a given channel from bot
        /// </summary>
        /// <param name="channel">Channel to send message to</param>
        /// <param name="message">Message to send</param>
        /// <returns></returns>
        public async Task SendMessage(string channel, string message) {
            await connected.Task;
            await tcpWriter.WriteLineAsync($"PRIVMSG #{channel} :{message}");
            Console.WriteLine($"SENT #{channel} {User}: {message}");
        }

        /// <summary>
        /// Joins a channel to read messages
        /// </summary>
        /// <param name="channel">Channel to join</param>
        /// <returns></returns>
        public async Task JoinChannel(string channel) {
            await connected.Task;
            await tcpWriter.WriteLineAsync($"JOIN #{channel}");
        }

        /// <summary>
        /// Leaves a channel to stop reading messages from there
        /// </summary>
        /// <param name="channel">Channel to leave</param>
        /// <returns></returns>
        public async Task LeaveChannel(string channel) {
            await connected.Task;
            await tcpWriter.WriteLineAsync($"PART #{channel}");
        }

        /// <summary>
        /// Graceful shutdown of the bot
        /// </summary>
        /// <returns></returns>
        public async Task Kill() {
            await SendMessage(User, "MrDestructoid 7 boop beep MrDestructoid 7");
            isAlive = false;
        }
    }
}
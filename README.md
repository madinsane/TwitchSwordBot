# TwitchSwordBot
C# Bot for twitch.tv primarily made for moderation including homoglyph parsing

# Download
Get the latest version from here: https://github.com/madinsane/TwitchSwordBot/releases

# Setup instructions
Get a new account on twitch (I advise against using your main channel for this)  
Login to your bot account  
Obtain an oauth key for the bot from https://twitchapps.com/tmi/  
Copy the oauth key  
Edit User.txt to have the bot's username in user field and oauth key in the pass  
E.g.  
  user:test_bot  
  pass:oauth:akx0438xmak09420  
Once User.txt is setup run TwitchSwordBot.exe whenever you want the bot to run and it will automatically join its own channel  
You can give the bot commands through its own channel, though you will need to moderate yourself when logged into the bot in its own channel (this prevents non-priviledged people using the bot)

# Commands
`!join <channel>` joins channel (required to read messages in that channel)  
`!leave <channel>` leaves channel (stop reading messages in that channel)  
`!msg <channel> <message>` or `!m <channel> <message>` send message to channel  
`!banword <phrase>` add word/phrase to ban list, will timeout any non-moderator for 30s on typing banned word  
`!unbanword <phrase>` remove word/phrase from ban list  
`!kill` shuts down the bot gracefully

# Build instructions
Only User.txt is excluded from this repo as it is used to contain bot login details  
User_Template.txt contains a template to use for this file if you are building it yourself (if using release build no need to do this)

# Contact
If theres any issues feel free to contact me either on discord madinsane#8324 or email madinsane@outlook.com

# References
Learned how to make the core bot using: https://medium.com/swlh/writing-a-twitch-bot-from-scratch-in-c-f59d9fed10f3  
Uses https://github.com/madinsane/discord-bot for homoglyph parsing and other string utils, forked from https://github.com/RPCS3/discord-bot

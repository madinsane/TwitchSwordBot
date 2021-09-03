# TwitchSwordBot
C# Bot for twitch.tv primarily made for moderation including homoglyph parsing

# Setup instructions
Get a new account on twitch (I advise against using your main channel for this)
Obtain an oauth key for the bot
Edit User.txt to have the bot's username in user field and oauth key in the pass  
E.g.  
  user:test_bot  
  pass:oauth:akx0438xmak09420  

# Build instructions
Only User.txt is excluded from this repo as it is used to contain bot login details  
User_Template.txt contains a template to use for this file if you are building it yourself (if using release build no need to do this)

# References
Learned how to make the core bot using: https://medium.com/swlh/writing-a-twitch-bot-from-scratch-in-c-f59d9fed10f3  
Uses https://github.com/madinsane/discord-bot for homoglyph parsing and other string utils, forked from https://github.com/RPCS3/discord-bot

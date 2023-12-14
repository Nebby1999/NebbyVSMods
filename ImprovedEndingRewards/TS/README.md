# Negative Max Health Fix - Stops the game setting your max health to negative, which happens for whatever reason.

## What the fuck is this?

Negative Max Health Fix is a mod that attempts to fix an issue where the game sometimes, for no clear reason, sets a characterbody's max health to a negative value.

## Care to be more of a nerd?

No, because genuenly i have no fucking idea why this happens at all.

It was said that apparently this bug was caused by Holy.dll's multithreaded code, however, it can continue happening even if you where using the "fixed" version that can be found on the discord server.

After hating this bug for so long, i've decided to try and find out a reason why this was happening by hooking directly into the MaxHealth's set method and recalculating stats if the incoming value was negative. While i couldn't find the reason, i did notice that my mod stopped at least 10 instances where the MaxHealth stat was being set to a negative value.

Oh also the mod will log a fatal error if MaxHealth is being set to negative, so if you find that in a log, feel free to send it my way, maybe we can find out what the fuck this is lol.

## Why?

This bug is so fucking shit there's no way to replicate it and literally kills good runs it sucks and i want to kill it.

## Donations

Feel free to give me a coffee here

[![ko-fi](https://media.discordapp.net/attachments/850538397647110145/994431434817273936/SupportNebby.png)](https://ko-fi.com/nebby1999)
# Stats Stunter -  Yet another desperate attempt to increase performance of the game

## What the fuck is this?

Stats Stunter is a mod that attempts to improve the performance of the game by changing how the stats manager handles it's stat processing queues.

## Care to be more of a nerd?

StatManager is a class in the game that handles events on a run, such as obtaining gold, dealing damage, taking damage, killing stuff, completing stages, etc.

This class itself tries to handles the events every single FixedUpdate, which theoretically is unececsary as most of the time most if not all queues are empty.

All this mod does is that the StatManager no longer handles events on FixedUpdate, but instead handles events whenever:

* The game gets paused
* The game is shutting down
* The current Scene changes (IE: Stage Transition)

This means that when the game gets paused or a scene changes you may experience a slight stutter if the queues are large enough, but theoretically the ingame experience wont get bogged down by large queues.

## Why?

This is a call for help i've been non stop developing my thesis videogame and i dearly miss making mods.

And also cuz performance ig, idk

## Does this fuck up my stat sheet on the long run?

Maybe! The mod itself works but i'm not liable if your stat sheets get fucked in the long run (If youre using mmods chances are your stats are already fucked beyond repair anyways lol)

## Donations

Feel free to give me a coffee here

[![ko-fi](https://media.discordapp.net/attachments/850538397647110145/994431434817273936/SupportNebby.png)](https://ko-fi.com/nebby1999)
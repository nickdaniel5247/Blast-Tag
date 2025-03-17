# Blast Tag

Blast Tag is a round based game where each round one player is eliminated. In the end, the last one standing wins. Every round a player is eliminated if they were the last one to be tagged; the game is essentially a dramatized game of tag. It is partially server authoritative and client authoritative. In terms of syncing player positions, it is client authoritative; however, when tagging other players and controlling the game logic flow, it is server authoritative. The game makes use of RPCs to time certain events such as sounds and Network Variables to sync things such as movement input from other players to help draw animations.

## How to Play

Simply enable virtual players with Multiplayer Play Mode, then start the game. On the chosen host, click **Create Lobby** and on the rest, click **Join Lobby**. Once all clients are connected, you can then start the game on the host by pressing **Enter**. To tag other players, you must face them close from behind or the side. The red player outline will indicate who is currently tagged.

## Controls

The only controls necessary is mouse input to press buttons on the UI, the **enter** button to start a game, and **WASD** to move.

## Sounds

There are 6 different sounds provided in this game of which they are a winning sound, 2 button variants, running, tagging, and a final countdown. The final countdown is a mashup of different sounds; however, just to be safe I added a button variant.
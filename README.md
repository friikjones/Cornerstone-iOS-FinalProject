# Dungeon Crawler
Dungeon Crawler is a free iOS game, built as Final Project for the iOS Development class of 2020 at CICCC

## What is the game?
The game is a action hack-n-slash, with procedural generated dungeons and run-based gameplay.
The player controls a character with emulated joystick and buttons on the screen.
Every playthrough is unique, as the layout of the dungeon is procedurally generated, as is the position and type of enemies encountered.
The game ends if the player gets to the final room or gets killed in the process.

## How is it made?
The game is built on Unity Engine, and deployed to iOS via Unity's deployment system.
The game consists in three main areas of development:
- Dungeon Generation
- Combat System
- User Experience (UX) and Input

### Dungeon Generation
This game uses a plugin for Unity called DungeonArchitect, that streamlines the dungeon procedure, making the development effort more focused in making a great, polished experience.
The plugin takes several assets, logic and input, as well as a seed, and produces a random generated dungeon, based on that seed.
The porpouse of having a seed is that the same seed creates the same dungeon, making multiplayer possible down the line.

### Combat System
The game features a hack-n-slash mechanic, where the character can move freelyin the enviroment while slashing around him.
The enemies are randomly generated along with the dungeon, while a simple AI that uses navmesh based movement calculation the best course of action against the player.
If the player hits enemies enough times they die, and vice-versa.

### User Experience (UX) and Input
This area encompasses the game's "flow", such as menu and end-game screens, UI, visual and sound notifications of important information, such as current life, getting hit, finding the end-room, etc.
The touch inputs also fall in this category. We use a emulated style button and joystick layout, as to emulate simple inputs in a non-intrusive manner as we don't have access to actual controller in a mobile enviroment.
This type of solution is one of the standarts for this type of game, and limit the creative process for the mechanics, as the input limitation reduces how many "verbs" the game can have.

## The team
The development team for this game is:
- Andre Mejia
- Daniel Jones
- Douglas Ciole

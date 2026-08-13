# Sentinels of Singapore

Every day, about **57 physical crimes** are reported. While this number may seem small, it represents only a fraction of offences that go unnoticed. Each of us has a role to play in keeping our community safe. 

Stand alert. Stay aware.  
Be a **Sentinel of Singapore** today!

## General Information
### Game Controls
- **WASD** to move or select
- **SPACE** to jump or enter
- **Hold** SHIFT to sprint
- **E** to interact
- **ESC** to pause or return
- **ENTER** to enter

### Non Playable Characters
Wrongly accusing innocent NPCs will lead to a deduction of points.

#### Civilian
- Moves from one point to another.
- Might shout if it notices a pickpocket stealing from them.

#### Pickpocket
- Chases a civilian. After committing the act, heads to a point to escape. 
- Might Run if it sees the player.

#### Vaper
- Moves to a point to vape.
- Will hold of vaping if player is nearby.
- Loses patience if the player stays near for too long and starts vaping.
- Might run if it sees the player.
- After vaping, heads to a point to escape.

#### Fighter
- Moves to a point and waits.
- At any moment coming into contact with another fighter, they will start fighting.
- Makes noise when fighting.
- The longer the fight, the lesser the score awarded when resolved.
- When the fight goes on too long, they despawn.

### Interactables
- NPCs
- Player police car stationed on the right side of the station's entrance
- Fighting Cloud

## Gameplay
- Run the SentinelsOfSingapore.exe file
- Choose Difficulty (Score penalty will be x1, x2, x3 based on the difficulty)
- Choose Quality (Low: Half the graphic resolution, Medium: Normal graphic resolution, High: Normal graphic resolution + shadows)
- Station may call periodically to reveal suspects' location if you accept the task. If the suspect is apprehended, additional 20 points will be awarded. If the suspect escapes, 20 points will be deducted.

## Game Hacks
### How the NPC AI works
There are spawn point and events points scattered around the level. NPCs spawns over time in waves that starts every minute. The first wave will spawn the remainder if the total enemies is not divisible by the play time fully.

Every NPC has a chance to stop, speed up, slow down, and change direction as they are walking.

- Pickpocket
    - Chases one civilian. If civilian leaves or get run over by the player, it will chase another active civilian.
    - Checks for collision with civilian and sets a random spawn point as a destination to escape.
    - Civilian will shout depending on their gender having different voices.
- Vaper
    - Goes to a random event point. If the player is within 30 units of the vaper, it will not smoke.
    - Vaper has a patience of 10 seconds before it starts vaping.
    - When smoking, it will constantly look around.
    - After vaping finish, it sets a random spawn point as a destination to escape.
- Fighter
    - Goes to a random event point and waits for its fighter pair. (Spawns in pairs)
    - Waits for 2 minutes.
    - If no fighter arrives because its pair is in a fight with another fighter or the fighter got run over by the player's car, it will set a random spawn point to leave. (Still innocent)
    - Fights for 1 minute, losing 1 point every 2 seconds.

### Cheats
- Certain values can be edited in the inspector depending on the object.
- Spawn points and event points can be placed on the level itself and changed.

### Pro tips
1. After accepting or rejecting the station call, the timer for the next call will start. However, the next call will not start until the revealed suspect is apprehended. Hence, try to catch the offender as soon as you reveal them.
2. The game is hard to get a good score so don't be discouraged. In the real world, many crimes still go unnoticed which is why we need more Sentinels of Singapore!

## Device Specifications
- Minimum: Windows 10, Intel i3, 4 GB RAM, integrated graphics.
- Recommended: Windows 10/11, Intel i5, 8 GB RAM, GTX 1050 or better.

## Limitations and Bugs
- The detection of the car driving on the road is not as accurate because of how the NavMesh is set up. The traffic light crossing is considered a non-drivable area for the NPCs to walk on it. So a point loss each time driving through the traffic light. Car parks is also considered not drivable.
- Occlusion culling causes the camera of the car to make objects disappear if it goes behind a wall.
- Car camera can go under the car and see the bottom of the level. Cannot do anything about it because it is imported and I don't understand it.
- Car flipping too easily. Since the car physics is imported, I don't know how to tweak this.
- The NPCs sometimes have the walking animation when it is staying still.

## Credits
- Car physics & controller - Ezereal
- Trees - forst
- Skybox - rpgwhitelock
- Street lights - 255 pixel studios
- Playground - ArtStudios3d
- Terrain textures - ALP
- Character models - Polygon Blacksmith
- Ceiling lights - PolyKebap 
- First person character controller - Unity Technologies
- Walkie Talkie - nikiteev
- Walkie Talkie Text Bubble - Vjom
- Asian Koel Sound - Pimnapat
- Chasing Music - RichHeard
- Background Music - kevp888
- Girl Scream - arjanvanhoorn0181
- Man Scream - klankbeeld
- People fighting - craigsmith
- Car engine - Cmart94
- Walkie Talkie sound - MiscPractice
- Handcuff sound - harrypeeks

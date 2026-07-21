
# What
Necroperator is a backup manager for Ghost Recon: Wildlands intended to keep ghost mode characters alive.

# Why
Ghost Mode has got some nice functionality that does not exist in the base game but perma death is also enforced by ghost mode.
It is still beneficial to keep backups even if perma death is desired because of how many instant death bugs there are in the game. Eg, the helicopter pilot disconnects and all passengers fall to their deaths.

# How
This is achieved by maintaining rolling backups triggered when the game writes to the local save files, this ensures that you always have the last 50 snapshots of your characters state. You'll have the option to roll back in small increments to find the last state before character loss.

# Usage
Disable cloud sync for GRW save files in the `Ubisoft Connect` client.

<img width="1860" height="903" alt="image" src="https://github.com/user-attachments/assets/f59dea6f-93e5-417b-8ac7-952c2280009d" />

Launch `Necroperator` and ensure that the correct path is entered for your saved games directory. By default it should be either:
```
C:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher\savegames\<accountId>\1771
``` 
or
```
C:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher\savegames\<accountId>\3559
```
Then press the start button.

<img width="768" height="498" alt="image" src="https://github.com/user-attachments/assets/03fa0e67-0300-492d-a859-3e8922a9d8e8" />

### On character death
1) Close `GRW`
2) Restore the latest backup
3) Relaunch `GRW`
4) Access Ghost Mode. If your character is still dead, repeat step 2 with an older backup

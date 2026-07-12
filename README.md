
# What
Necroperator is a backup manager for Ghost Recon: Wildlands intended to keep ghost mode characters alive.

# Why
Ghost Mode has got some nice functionality that does not exist in the base game but perma death is also enforced by ghost mode.
It is still beneficial to keep backups even if perma death is desired because of how many instant death bugs there are in the game. Eg, the helicopter pilot disconnects and all passengers fall to their deaths.

# How
This is achieved by making a last second backup as the character save is marked for deletion. Once the deletion is complete we automatically replace the old files with the backup.

This is not 100% successful as it is subject to race-conditions (sometimes GRW will be able to delete before we are able to back it up) so we also keep a rolling backup every 10 minutes, which can then be manually copied if needed.

<img width="427" height="624" alt="image" src="https://github.com/user-attachments/assets/68dbe29b-1aac-4ee5-ac4f-033ba912d6e8" />

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

<img width="1249" height="647" alt="image" src="https://github.com/user-attachments/assets/4e455340-bd77-4da2-a46d-d36b6e00425b" />

After a character has died and a backup is restored GRW will need to be relaunched for the character to appear in the list.

**NOTE**: _Leaving this running after your character has died would eventually overwrite your periodic backups (after 200 minutes) so be sure to resolve your character's death or stop `Necroperator`._

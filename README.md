# FileManager_CSharp_Console
File manager written on C# 

File Manager description:

1.) Interface

First time app runs, it will adapt for the screen resolution.
It takes screen resolution values and builds the interface,
the number and location of interactive buttons, and so on. 

![alt text](https://github.com/Sturmik/FileManager_CSharp_Console/blob/master/ShowcaseImages/MainMenu2.PNG?raw=true)

*I carried out checks in different resolutions and the interface remained completely readable.
However, adaptation occurs only at the first start. 
So I strongly do not recommend changing its size to a smaller one, 
but the opposite is entirely acceptable up to expanding the console window to full screen.

2.) First launch

At the first start, you will be asked to register a user - the administrator,
who will have the greatest rights among other users and have them individually.

![alt text](https://github.com/Sturmik/FileManager_CSharp_Console/blob/master/ShowcaseImages/LoginIn.PNG?raw=true)

The registration process is very simple. The user is required to enter a login and password, 
which he will use to enter the programm in the future.

After registration, you will be taken to the main menu window, 
where you can log in as a user, register, or exit the application.

3.) File Manager functions

After successful login, you will gain access to the file manager main working window:

1. Top row of function buttons:

![alt text](https://github.com/Sturmik/FileManager_CSharp_Console/blob/master/ShowcaseImages/MainFunctions.PNG?raw=true)

- Open (Responsible for navigating in disks and file directories, also allows you to open and run files)
- Back (Moves you one step back, more simply put, to the previous directory)
- Find (Allows you to find the file or directory you need by a specific parameter, starting from your current position)
- Copy | ADMIN | (Allows you to copy an object by placing it in your buffer. If this button is highlighted,
it means that the file is still in the buffer. When you press it again, it copies the file from the buffer to your current position)
- Move | ADMIN | (Almost the same as the previous button, except that it moves the file to a new location)
- Delete | ADMIN | (Allows you to delete a file or directory)
- Rename | ADMIN | (Allows you to rename the file)

* | ADMIN | - indicates functions, which are available only for admin user

User Settings:
- Most viewed (General statistics for viewing certain directories, files. Displayed from most viewed to less visited)
- History
- Change User (Allows you to change the user)

Navigation in the file manager:
- Up (moves your selector up a division)
- Down (moves your selector one division down)
- Page back (moves you to the previous page)
- Page forward (moves you to the next page)

2. Location. It is located under the function buttons and displays the current position of the user in directories.
Also, in special cases, displays information that the user is viewing a history or statistics.

![alt text](https://github.com/Sturmik/FileManager_CSharp_Console/blob/master/ShowcaseImages/InfoWindow.PNG?raw=true)

3. The left window for selecting logical drives. Allows you to select the drive with which user will interact.
The disc selected by the user is highlighted. Below this window is a small square that displays your current page.

![alt text](https://github.com/Sturmik/FileManager_CSharp_Console/blob/master/ShowcaseImages/DiscWindow.PNG?raw=true)

4. Window displaying the current status.
It is located under the logical drive selection window and displays useful information for the user.

![alt text](https://github.com/Sturmik/FileManager_CSharp_Console/blob/master/ShowcaseImages/StatusWindow.PNG?raw=true)

5. The right window for selecting files and directories. 
Allows you to interact with files and directories that are on the disk. 
Also used to display a list of statistics and history. The selected user file is highlighted. 
Similar to the logical drive window, there is a small red square that displays the current page.

![alt text](https://github.com/Sturmik/FileManager_CSharp_Console/blob/master/ShowcaseImages/FileWindow.PNG?raw=true)

6. Information. 
The lowest information board where information about the current file or directory is displayed 
(file size, its access, and so on)

![alt text](https://github.com/Sturmik/FileManager_CSharp_Console/blob/master/ShowcaseImages/FileInfo.PNG?raw=true)

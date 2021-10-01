# **Documentation static website editing workflow**

**Prerequisites**

1 -	Install Git [Git - Downloads](https://git-scm.com/downloads)

2 - Install Visual Studio Code  [Visual Studio Code - Code Editing. Redefined](https://code.visualstudio.com/)

3 - Install Node.js [Download | Node.js](https://nodejs.org/en/download/)

4 - Install Hugo [Hugo](https://gohugo.io/getting-started/installing/)

**Adding HUGO to environment variables to make it available globally - OPTIONAL (Requires admin privilege)**

From Windows:

****

-	Right click on the Start button
-	Click on System
-	Click on Advanced System Settings on the right
-	Click on the Environment Variables… button on the bottom
-	In the User variables section, find the row that starts with PATH
-	Double click on PATH
-	Click the New… button
-	Type in the folder where hugo.exe was copied C:\Hugo\bin if you went by the instructions above. The PATH entry should be the folder where Hugo lives and not the binary. Press Enter when you’re done typing.
-	Click OK at every window to exit.
-	Verify that Hugo is ready to run by opening the Command Prompt (just type cmd in your Windows search bar and hit Enter). At the prompt type hugo help and press the Enter key: 
 

You should see a list of commands available, if you do the installation is complete.

**Run the documentation website locally and preview changes**

- From within the **documentation** folder in Visual Studio Code click on Terminal -> New Terminal
- A new command line will be prompted at the bottom: 
IF you have added hugo to your system path environment variables with the instructions above, run the command **hugo server**
**otherwise** run the command **deploy/hugo server**  <- (which is the folder that contains the hugo.exe), this will build the website and make it available locally at http://locahost:1313/

-	It will listen for any changes you make through Visual Studio Code and will refresh the page accordingly every time you save a change (Ctrl+S in VS Code)
-	You can stop hugo server simply by pressing Ctrl+C from within your command prompt, it will stop listening for changes and stop the local webserver at http://localhost:1313
-	Once you are happy with your local changes, you can commit them, then proceeding on creating a pull request for them in order to have it published on the website.


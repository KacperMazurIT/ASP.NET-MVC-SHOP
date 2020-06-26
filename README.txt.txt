HOW TO RUN THIS PROJECT ON YOUR DEVICE?

There u have some clue :)

1. Clone Repository

2. Restore your packages

3. Write this command in your console "Update-Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform -r"


4.If u using migrations write this to your console!

(1 command) "enable-migrations"
(2 second command) "add-migration InitialCreate"

5.After u should delete App_Data folder and add it again.

How u can add App_Data folder?

Solution

Go to Solution Explorer -> Right Click on your project -> Add -> Add Asp.NET Folder -> App_Data

6. After this u can write in your console command

"update-database"

## Demo
![Demo](https://i.imgur.com/9T1xzY7.png)
## Running this project
* Clone this repository:
```
    $ git clone https://github.com/Horikaze/CS16_RPC
```
* Include the [DiscordRichPresence](https://www.nuget.org/packages/DiscordRichPresence) and [swed32](https://www.nuget.org/packages/swed32) :
```
    PM> Install-Package DiscordRichPresence swed32
```
 * Build the project to produce the CS1.6 RPC console app.
```
     $ dotnet publish -r win-x86 -c Release
```
 * Run app ```bin\Release\net8.0\win-x86\publish\csrpc.exe```
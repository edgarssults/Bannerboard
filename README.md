# Description

This is a mod for Mount &amp; Blade II: Bannerlord that lets you view your game data on a dashboard in a browser. It is a work in progress.

When the mod is loaded it sets up a local WebSocket server and pushes updates to connected clients.
The client is a Blazor WebAssemmbly app that runs in a browser and connects to the local server and listens for updates.

No data is sent outside your system, the browser downloads the client code and runs it locally.

# Screenshots

![Dashboard](screenshots/dashboard.png)

Currently the dashboard has only one widget which displays a kingdom strength chart. First kingdom is the player, rest are NPCs. The chard updates every in-game hour.

# Links

Download the mod: TODO

Connect to your game: https://bannerlorddashboard.azurewebsites.net/

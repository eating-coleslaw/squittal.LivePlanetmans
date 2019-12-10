# squittal.LivePlanetmans

Planetside 2 live activity stats and leaderboard.

![Leaderboard and Player Details Preview](https://github.com/eating-coleslaw/squittal.LivePlanetmans/blob/master/images/FullPreview.gif))

## Requirements

### .NET Core 3.1 SDK

The .NET Core 3.1 SDK and Runtime (the runtime is included with the SDK) is required to build and run the app. [Download the latest version of the .NET Core 3.1 SDK from the downloads page.](https://dotnet.microsoft.com/download/dotnet-core/3.0 "Download .NET Core 3.0")

### SQL Server 2017 Express edition

This is the database provider used to store all data for the app.

1. Download and run the installer from [Microsoft's website](https://www.microsoft.com/en-us/sql-server/sql-server-editions-express "SQL Server 2017 Express edition download page").

     * Note the location to which the installer extracted the setup files.

2. If SQL Server Installation Center did not automatically open, navigate to the folder where the installer extracted the setup files (SQLEXPR_x64_ENU or SQLEXPR_x86_ENU) and run SETUP.EXE.

3. Select `New SQL Server stand-alon installation or add features to an existing installation`.

4. Advance through the installer until reaching the __Installation Type__ page. Select `Perform a new installation of SQL Server 2017`.

5. On the __Feature Select__ page, check `Database Engine Services` and, optionally, `SQL Server Replication`. Change the the `Instance root directory` if you want.

6. On the __Instace Configuration__ page, set both the `Named instance` and `Instance ID` fields to "SQLEXPRESS" (without quotes).

7. Leave the __Server Configuration__ with its default values.

8. If running the app on your own local computer, select `Windows authentication mode` on the __Database Engine Configuration__ page and `Add Current User` to the list of SQL Server administrators. _If you're running the app in some other manner, I can't help you._

9. Continue advancing through the installer until it begins the actual installation. Once completed, it will show whether the installation succeeded.

10. It's not strictly required, but I'd recommend now downloading and installing [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?redirectedfrom=MSDN&view=sql-server-ver15 "Download SQL Server Management Studio (SSMS)").

### Daybreak Games Service ID

Using a registered Service ID permits unthrottled querying of the Census API. Without one, you're limited to 10 queries per minute (e.g. 10 character lookups). This app greatly exceeds that limit, so you will need to get your own Service ID.

1. Apply for a service ID on the [DBG Census API website](http://census.daybreakgames.com/#devSignup). You'll receive an email notification once your request has been processed (I received mine within a few hours, but your results may vary).

2. Open `Control Panel > System and Security > System > Advanced system settings`. In the __System Properties__ window that opens, select the `Environment Variables...` button on the Advanced tab. The __Environment Variables__ window will open.

3. Under __User veriables for *username*__, select `New..`. to add a new user variable with name "DaybreakGamesServiceKey" (without the quotes) and a value of your census API service key. Click OK to accept the new variable.

### _(For Development Only)_ Visual Studio 2019 (v16.4)

.NET Core 3.1 is only compatible with Visual Studio 2019 (v16.4). Download the free community edition [here](https://visualstudio.microsoft.com/free-developer-offers/ "Visual Studio Preview download page").

## Running the App

1. Open the folder `squittal.LivePlanetmans\squittal.LivePlanetmans\squittal.LivePlanetmans\Server` in a command prompt window.

   * `Shift + Right Click > Open command window here`, or
   * `Shift + Right Click > Open PowerShell window here`

2. If it's the first time running the app, or you just synced changes from the repository, enter `dotnet build` to build the app.

3. Enter `dotnet run` to start the app.

   Note: The app will continue to collect Planetside 2 activity data while the app is running, regardless of whether you have the site open in a browser.

4. In your web browser navigate to the site displayed after the `Now listening on: ...` console message (e.g. <http://localhost:55572>).

## Maintenance

Any given database instance of SQL Server 2017 Express is limited to 10gb in size. If running the app fairly frequently, you'll occasionally need to delete old data. Run `squittal.LivePlanetmans\Server\Data\SQL\RemoveOldData.sql` on your databse to delete event data (deaths, logins, logouts) more than 2 days old and rebuild the associated indexes.

## Troubleshooting

If you don't see your issue below, please write up an Issue.

### SQL Server Instance Not Found or Not Accessible

When attempting to run the app, you get an error message like this:  
`An error occured initializing the DB.
Microsoft.Data.SqlClient.SqlException (0x80131904): A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: SQL Network Interfaces, error: 26 - Error Locating Server/Instance Specified)`

This means that the SQL Database service has been stopped for some reason. Manually start the service, then try running the again.

1. Open the __Services__ Windows app.

2. Scroll down to __SQL Server (SQLEXPRESS)__. If the service has stopped, the Status column value will be blank.

3. Select Start from the row's right-click menu to restart the service.

### SQL Server Using Excessive Resources

The SQL Server Windows NT process started suddenly taking up a large amount of CPU, Memory, or Disk resources even though you're not currently running the app.

The SQL database & associated service are independant of the leaderboard app itself, and so they'll continue to run after the app is stopped. Manually stop the SQL Server service after closing the app, and restart it before running the app again.

#### Stop Service via Services App

1. Open the __Services__ Windows app.

2. Scroll down to __SQL Server (SQLEXPRESS)__. If the service is running, it will show _Running_ in the Status column.

3. Select `Stop` from the row's right-click menu to stop the service.

#### Stop Service via Task Manager

1. Ending the _SQL Server Windows NT_ process in __Task Manager__ will stop the appropriate services, freeing up system resources.

#### Set Service to Manual Startup

1. If the __SQL Server (SQLEXPRESS)__ service is set to start with your computer, it will show _Automatic_ under the Startup Type column. If you don't want this behavior, right-click and select `Properties`.

2. Set Startup type to _Manual_. 

   You will need to ensure the service is running each time before starting the leaderboard app: select `Start` from the right-click menu.

## Credits & Technologies

This is a project for me to learn C# & .NET, re-learn OOP, and practice designing reporting business logic and dashboard UI.

* Backend is largely straight from Lampjaw's  [Voidwell.com](https://github.com/voidwell/Voidwell.DaybreakGames "Voidwell's backend github repository"), with some small modifications by me. Interacting with the Daybreak Games Census API and event streaming service are done with Lampjaw's [DaybreakGames.Census NuGet package](https://github.com/Lampjaw/DaybreakGames.Census "DaybreakGames.Census package github repository").
* Frontend/Client is ASP.NET Core Blazor (3.0.0-preview9.19457.4).
* UI is designed by me, with inspiration from [fisu](ps2.fisu.pw) and [Voidwell](Voidwell.com).
* Business logic is designed by me.

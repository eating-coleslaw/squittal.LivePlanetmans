# squittal.LivePlanetmans

Planetside 2 live activity stats and leaderboard.

## Requirements

* __.NET Core SDK 3.0.100-preview9-014004__
  
  * [Download the appropriate version for your OS from the .NET Core 3.0 downloads page.](https://dotnet.microsoft.com/download/dotnet-core/3.0 "Download .NET Core 3.0")
  * Note: Later preview version of .NET Core 3.0 _may_ work, but have not been tested.

* __SQL Server 2017 Express edition__

    1. Download and run the installer from [here](https://www.microsoft.com/en-us/sql-server/sql-server-editions-express "SQL Server 2017 Express edition download page").
    2. SQL Server Installation Center should have automatically opened. If not, navigate to the folder where the installer extracted the setup files (SQLEXPR_x64_ENU or SQLEXPR_x86_ENU) and run SETUP.EXE.
    3. Select __New SQL Server stand-alon installation or add features to an existing installation__.
    4. Advance through the installer until reaching the __Installation Type__ page. Select __Perform a new installation of SQL Server 2017__.
    5. On the __Feature Select__ page, check __Database Engine Services__ and, optionally, __SQL Server Replication__. Change the the __Instance root directory__ if you want.
    6. On the __Instace Configuration__ page, set both the __Named instance__ and __Instance ID__ fields to "SQLEXPRESS" (without quotes).
    7. Leave the __Server Configuration__ with its default values.
    8. If running the app on your own local computer, select __Windows authentication mode__ on the __Database Engine Configuration__ page and Add Current User to the list of SQL Server administrators. If you're running the app in some other manner, I can't help you.
    9. Continue advancing through the installer until it begins the actual installation. Once completed, it will show whether the installation succeeded.
    10. It's not strictly required, but I'd recommend now downloading and installing [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?redirectedfrom=MSDN&view=sql-server-ver15 "Download SQL Server Management Studio (SSMS)").

* __A Daybreak Games Service ID__

   1. Apply for a service ID on the [DBG Census API website](http://census.daybreakgames.com/#devSignup). You'll receive an email notification once your request has been processed (I received mine within a few hours).
   2. Open Control Panel > System and Security > System > Advanced system settings. In the System Properties window that opens, select the Environment Variables... button on the Advanced tab. The Environment Variables window will open.
   3. Under __User veriables for \<username>__, select New... to add a new user variable with name "DaybreakGamesServiceKey" (without the quotes) and a value of your census API service key. Click OK to accept the new variable.

* __Visual Studio 2010 Preview__ [_For Development Only_]

  * Download the community edition [here](https://visualstudio.microsoft.com/vs/preview/ "Visual Studio Preview download page").

## Running the App

1. Open the folder squittal.LivePlanetmans\squittal.LivePlanetmans\squittal.LivePlanetmans\Server in a command prompt window (Shift + Right Click > Open command window here, or Open PowerShell window here).
2. Enter `dotnet run` to start the app.
3. In your web browser navigate to the site displayed after the "Now listening on: ..." console message (e.g. <http://localhost:55572>).

## Maintenance

Any given database instance of SQL Server 2017 Express is limited to 10gb in size. If running the app fairly frequently, you'll occasionally need to delete old data. Run `squittal.LivePlanetmans\Server\Data\SQL\RemoveOldData.sql` on your databse to delete event data (deaths, logins, logouts) more than 2 days old and rebuild the associated indexes.

## Troubleshooting

If you don't see your issue below, please write up an Issue.

1. `An error occured initializing the DB.
Microsoft.Data.SqlClient.SqlException (0x80131904): A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: SQL Network Interfaces, error: 26 - Error Locating Server/Instance Specified)`
   * __Cause:__ the SQL Database service has been stopped for some reason.
   * __Fix:__ Restart the service.
     1. Open the __Services__ Windows app.
     2. Scroll down to __SQL Server Agent (SQLEXPRESS)__. If the service has indeed been stopped, it will have no value in the Status column.
     3. Select Start from the row's right-click menu to restart the service.

1. A process called SQL Server Windows NT is taking up a huge amount of CPU, Memory, or Disk even though you're not currently running the app.
   * __Cause:__ The SQL database & associated service run independently from the leaderboard app itself, so you need to start and stop them separately.
   * __Fix Pt 1:__ Manually stop the SQL service.
     1. Open the __Services__ Windows app.
     2. Scroll down to __SQL Server Agent (SQLEXPRESS)__. If the service is running, it will show Running in the Status column.
     3. Select Stop from the row's right-click menu to stop the service.
   * __Fix Pt 2:__ Set the service to not automatically start with your computer.
     1. If the service is set to start with your computer, it will show Automatic under the Startup Type parameter. If you don't want this behavior, right-click and select Properties.
     2. Set Startup type to Manual. You will need to ensure the service is running each time before starting the leaderboard app.

## Credits & Technologies

This is a project for me to learn C# & .NET, re-learn OOP, and practice designing reporting business logic and dashboard UI.

* Backend is largely straight from Lampjaw's  [Voidwell.com](https://github.com/voidwell/Voidwell.DaybreakGames "Voidwell's backend github repository"), with some small modifications by me. Interacting with the Daybreak Games Census API and event streaming service are done with Lampjaw's [DaybreakGames.Census NuGet package](https://github.com/Lampjaw/DaybreakGames.Census "DaybreakGames.Census package github repository").
* Frontend/Client is ASP.NET Core Blazor (3.0.0-preview9.19457.4).
* UI is designed by me, with inspiration from [fisu](ps2.fisu.pw) and [Voidwell](Voidwell.com).
* Business logic is designed by me.

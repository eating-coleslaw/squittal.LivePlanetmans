USE PlanetmansDbContext;

DECLARE @dTodayUtc         date,
        @dStartUtc         date,
        @iDaysToKeep       int,
        @iDeleteDaysOffset int,
        @vMessage          nvarchar(150);

BEGIN
SET @iDaysToKeep = 2;
SET @iDeleteDaysOffset = @iDaysToKeep * -1;

SET @dTodayUtc = CONVERT( date, GETUTCDATE() );
  
SET @dStartUtc = DATEADD( day, @iDeleteDaysOffset, @dTodayUtc );
  
SET @vMessage = N'Deleting events prior to ' + CAST( @dStartUtc as nvarchar(30));
RAISERROR( @vMessage, 0, 1 ) WITH NOWAIT;

  -- DeathEvent
  BEGIN
  RAISERROR( N'Started deleting Death events', 0, 1 ) WITH NOWAIT;
  DELETE FROM DeathEvent
         WHERE [Timestamp] < @dStartUtc;
  RAISERROR( N'Finished deleting Death events', 0, 1 ) WITH NOWAIT;

  RAISERROR( N'Started rebuilding DeathEvent indexes', 0, 1 ) WITH NOWAIT;
  ALTER INDEX ALL ON DeathEvent REBUILD;
  RAISERROR( N'Finished rebuilding DeathEvent indexes', 0, 1 ) WITH NOWAIT;
  END;

  -- Login Events
  BEGIN
  RAISERROR( N'Started deleting Login events', 0, 1 ) WITH NOWAIT;
  DELETE FROM PlayerLoginEvent
         WHERE [Timestamp] < @dStartUtc;
  RAISERROR( N'Finished deleting Login events', 0, 1 ) WITH NOWAIT;

  RAISERROR( N'Started rebuilding LoginEvent indexes', 0, 1 ) WITH NOWAIT;
  ALTER INDEX ALL ON PlayerLoginEvent REBUILD;
  RAISERROR( N'Finished rebuilding LoginEvent indexes', 0, 1 ) WITH NOWAIT;
  END;

  -- Logout Events
  BEGIN
  RAISERROR( N'Started deleting Logout events', 0, 1 ) WITH NOWAIT;
  DELETE FROM PlayerLogoutEvent
         WHERE [Timestamp] < @dStartUtc;
  RAISERROR( N'Finished deleting Logout events', 0, 1 ) WITH NOWAIT;

  RAISERROR( N'Started rebuilding LogoutEvent indexes', 0, 1 ) WITH NOWAIT;
  ALTER INDEX ALL ON PLayerLogoutEvent REBUILD;
  RAISERROR( N'Finished rebuilding LogoutEvent indexes', 0, 1 ) WITH NOWAIT;
  END;

END;
USE PlanetmansDbContext;

SET NOCOUNT ON;

DECLARE @iKillType     int         =  0,
        @iTeamKillType int         =  1,
        @iSuicdeType   int         =  2,
        @iDefaultValue int         = -1, --Default value from migration
        @vNoAttackerId nvarchar(1) = '0';

-- Column exists and caller has permission to view the object
If COL_LENGTH('DeathEvent', 'DeathEventType') IS NOT NULL
BEGIN

  -- Kills & Teamkills
  UPDATE DeathEvent
    SET DeathEventType = CASE WHEN AttackerFactionId = CharacterFactionId THEN @iTeamKillType
                              ELSE @iKillType END
    WHERE DeathEventType = @iDefaultValue 
      AND AttackerCharacterId <> @vNoAttackerId
      AND AttackerCharacterId <> CharacterID;

  -- Suicides
  UPDATE DeathEvent
    SET DeathEventType = @iTeamKillType
    WHERE DeathEventType = @iDefaultValue
      AND ( ( AttackerCharacterId <> @vNoAttackerId
                AND AttackerCharacterId = CharacterId )
            OR AttackerCharacterId = @vNoAttackerId );

END;

SET NOCOUNT OFF;
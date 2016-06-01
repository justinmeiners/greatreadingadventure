﻿CREATE PROCEDURE [dbo].[rpt_ProgramByBranchPrelogging] @TenID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT pgm.[TabName] AS [Program],
		count(p.[pid]) AS [Signups],
		sum(CASE [dbo].[fx_IsFinisher2](p.PID, p.ProgID, NULL)
				WHEN 1
					THEN 1
				ELSE 0
				END) AS [Achievers],
		pgm.[CompletionPoints] AS [Achiever Points]
	FROM [Programs] pgm
	LEFT OUTER JOIN [patron] p ON p.[ProgID] = pgm.[PID]
		AND p.[RegistrationDate] < pgm.[LoggingStart]
	WHERE p.[TenId] = @TenID
	GROUP BY pgm.[TabName],
		pgm.[PID],
		pgm.[CompletionPoints]
	ORDER BY pgm.[PID]

	DECLARE @ProgramId INT
	DECLARE @LoggingStart DATETIME

	DECLARE PGM_CURSOR CURSOR LOCAL STATIC READ_ONLY FORWARD_ONLY
	FOR
	SELECT [PID],
		[LoggingStart]
	FROM [Programs]
	ORDER BY [PID]

	OPEN PGM_CURSOR

	FETCH NEXT
	FROM PGM_CURSOR
	INTO @ProgramId,
		@LoggingStart

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT coalesce(s.[Description], 'No System') AS [Library System],
			coalesce(b.[description], 'No Branch') AS [Library],
			count(p.[pid]) AS [Signups],
			sum(CASE [dbo].[fx_IsFinisher2](p.PID, p.ProgID, NULL)
					WHEN 1
						THEN 1
					ELSE 0
					END) AS [Achievers]
		FROM [code] b
		INNER JOIN [librarycrosswalk] lxw ON lxw.[BranchId] = b.[CID]
		INNER JOIN [code] s ON lxw.[DistrictId] = s.[CID]
			AND s.[CTID] IN (
				SELECT [CTID]
				FROM [CodeType]
				WHERE [CodeTypeName] = 'Library District'
					AND [TenID] = @TenID
				)
		LEFT OUTER JOIN [patron] p ON p.[PrimaryLibrary] = b.[CID]
			AND p.[ProgID] = @ProgramId
			AND p.[TenID] = @TenID
			AND p.[RegistrationDate] < @LoggingStart
		WHERE b.[CTID] IN (
				SELECT [CTID]
				FROM [CodeType]
				WHERE [CodeTypeName] = 'Branch'
					AND [TenID] = @TenID
				)
		GROUP BY s.[Description],
			b.[description]
		ORDER BY s.[Description],
			b.[description]

		FETCH NEXT
		FROM PGM_CURSOR
		INTO @ProgramId,
			@LoggingStart
	END

	CLOSE PGM_CURSOR

	DEALLOCATE PGM_CURSOR
END
﻿
--Create the Insert Proc
CREATE PROCEDURE [dbo].[app_Offer_Insert] (
	@isEnabled BIT,
	@AdminName VARCHAR(50),
	@Title VARCHAR(150),
	@ExternalRedirectFlag BIT,
	@RedirectURL VARCHAR(150),
	@MaxImpressions INT,
	@TotalImpressions INT,
	@SerialPrefix VARCHAR(50),
	@ZipCode VARCHAR(5),
	@AgeStart INT,
	@AgeEnd INT,
	@ProgramId INT,
	@BranchId INT,
	@LastModDate DATETIME,
	@LastModUser VARCHAR(50),
	@AddedDate DATETIME,
	@AddedUser VARCHAR(50),
	@TenID INT = 0,
	@FldInt1 INT = 0,
	@FldInt2 INT = 0,
	@FldInt3 INT = 0,
	@FldBit1 BIT = 0,
	@FldBit2 BIT = 0,
	@FldBit3 BIT = 0,
	@FldText1 TEXT = '',
	@FldText2 TEXT = '',
	@FldText3 TEXT = '',
	@OID INT OUTPUT
	)
AS
BEGIN
	INSERT INTO Offer (
		isEnabled,
		AdminName,
		Title,
		ExternalRedirectFlag,
		RedirectURL,
		MaxImpressions,
		TotalImpressions,
		SerialPrefix,
		ZipCode,
		AgeStart,
		AgeEnd,
		ProgramId,
		BranchId,
		LastModDate,
		LastModUser,
		AddedDate,
		AddedUser,
		TenID,
		FldInt1,
		FldInt2,
		FldInt3,
		FldBit1,
		FldBit2,
		FldBit3,
		FldText1,
		FldText2,
		FldText3
		)
	VALUES (
		@isEnabled,
		@AdminName,
		@Title,
		@ExternalRedirectFlag,
		@RedirectURL,
		@MaxImpressions,
		@TotalImpressions,
		@SerialPrefix,
		@ZipCode,
		@AgeStart,
		@AgeEnd,
		@ProgramId,
		@BranchId,
		@LastModDate,
		@LastModUser,
		@AddedDate,
		@AddedUser,
		@TenID,
		@FldInt1,
		@FldInt2,
		@FldInt3,
		@FldBit1,
		@FldBit2,
		@FldBit3,
		@FldText1,
		@FldText2,
		@FldText3
		)

	SELECT @OID = SCOPE_IDENTITY()
END

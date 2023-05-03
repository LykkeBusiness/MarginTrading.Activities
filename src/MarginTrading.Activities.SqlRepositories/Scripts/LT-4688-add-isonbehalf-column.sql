-- Add IsOnBehalf column to Activities table

ALTER TABLE [dbo].Activities
ADD IsOnBehalf BIT NOT NULL DEFAULT 0;
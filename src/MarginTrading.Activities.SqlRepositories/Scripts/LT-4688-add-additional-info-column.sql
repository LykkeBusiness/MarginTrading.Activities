-- Add AdditionalInfo column to Activities table

ALTER TABLE [dbo].Activities
ADD AdditionalInfo [nvarchar](MAX) NULL;
---
external help file: DNSDebugLogHandler.dll-Help.xml
Module Name: DNSDebugLogHandler
online version:
schema: 2.0.0
---

# Import-DNSDebugLog

## SYNOPSIS
This imports DNS Debug log files.

## SYNTAX

```
Import-DNSDebugLog [-Path] <String> [[-Culture] <CultureInfo>] [<CommonParameters>]
```

## DESCRIPTION
{{ Fill in the Description }}

## EXAMPLES

### Example 1
```powershell
PS C:\> Import-DNSDebugLog logfile.txt
```

Imports a DNS Debug logfile

### Example 2
```powershell
PS C:\> Import-DSNDebugLog logfile.txt -Culture [cultureinfo]::new("de-de")
```

Imports a DNS Debug log file with a defined culture.
This is needed in Case the time format of the DNS Debug log file is different than the current culture.

## PARAMETERS

### -Path
Path to the log file

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -Culture
Culture to use when parsing the timestamps in the log file

```yaml
Type: System.Globalization.CultureInfo
Parameter Sets: (All)
Aliases:

Required: False
Position: None
Default value: System.Globalization.CultureInfo.CurrentCulture
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String

## OUTPUTS

### DNSDebugLogHandler.DNSLogEntry
## NOTES

## RELATED LINKS

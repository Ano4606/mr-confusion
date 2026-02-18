# CSV Parsing Fix - Handling Commas in Text

## Problem

CSV files use commas to separate columns. When the text content itself contains commas, it breaks the simple `Split(',')` parsing.

### Example Problem:

```csv
"P01","Group1",2,"I","Absolutely. I listen to a podcast every morning to see what's happened in the world. Otherwise, I feel out of the loop before breakfast is even over.","I_2.mp3"
```

The text has commas: "...morning to see what's happened in the world. Otherwise, I feel..."

Simple `Split(',')` would incorrectly split this into too many columns.

## Solution

Implemented proper CSV parsing that:
1. Tracks when we're inside quoted fields
2. Ignores commas inside quotes
3. Only splits on commas outside quotes

### New Method: `ParseCSVLine()`

```csharp
private string[] ParseCSVLine(string line)
{
    var result = new List<string>();
    bool inQuotes = false;
    string currentField = "";

    for (int i = 0; i < line.Length; i++)
    {
        char c = line[i];

        if (c == '"')
        {
            inQuotes = !inQuotes;  // Toggle quote state
        }
        else if (c == ',' && !inQuotes)
        {
            result.Add(currentField);  // End of field
            currentField = "";
        }
        else
        {
            currentField += c;  // Add to current field
        }
    }

    result.Add(currentField);  // Add last field
    return result.ToArray();
}
```

## How It Works

### Example 1: Simple Line (No Commas in Text)
```csv
"P01","Group1",1,"SA","Hello world","SA_1.mp3"
```

Parsed as:
- Column 0: P01
- Column 1: Group1
- Column 2: 1
- Column 3: SA
- Column 4: Hello world
- Column 5: SA_1.mp3

### Example 2: Text with Commas
```csv
"P01","Group1",2,"I","Yes, I agree, it's great","I_2.mp3"
```

Parsed as:
- Column 0: P01
- Column 1: Group1
- Column 2: 2
- Column 3: I
- Column 4: Yes, I agree, it's great  ← Commas preserved!
- Column 5: I_2.mp3

## CSV Format Requirements

Your CSV should follow this format:

```csv
"Participant","Group","Line","Speaker","Text","AudioFile"
"P01","Group1",1,"SA","Text can have, commas, and it works!","SA_1.mp3"
"P01","Group1",2,"I","Another line, with commas.","I_2.mp3"
"P01","Group1",3,"P","Player response",NA
```

### Rules:
1. ✅ Text fields should be in quotes: `"Text here"`
2. ✅ Commas inside quotes are preserved
3. ✅ Quotes inside text should be escaped: `"He said ""hello"""`
4. ✅ NA for no audio file

## Debug Output

When loading the CSV, you'll see:

```
[CSV] Loading conversation for participant: P01
[CSV] Line 1: Speaker=SA, Text='Have you noticed how today's...', Audio=SA_1.mp3
[CSV] Line 2: Speaker=I, Text='Absolutely. I listen to a pod...', Audio=I_2.mp3
[CSV] Line 3: Speaker=P, Text='Right now, local politics is ...', Audio=NA
[CSV] Total lines loaded for P01: 30
```

If a line has parsing issues:
```
[CSV] Line 15 has only 4 columns, expected 6. Skipping.
```

## Testing Your CSV

To verify your CSV is formatted correctly:

1. Open in a text editor (not Excel)
2. Check that text with commas is in quotes
3. Count the commas on each line (should be 5 commas per line)
4. Run the game and check Console for `[CSV]` messages

## Common CSV Issues

### Issue 1: Missing Quotes
```csv
P01,Group1,1,SA,Text with, commas,SA_1.mp3  ← WRONG (7 columns)
"P01","Group1",1,"SA","Text with, commas","SA_1.mp3"  ← CORRECT (6 columns)
```

### Issue 2: Unescaped Quotes in Text
```csv
"P01","Group1",1,"SA","He said "hello"","SA_1.mp3"  ← WRONG
"P01","Group1",1,"SA","He said ""hello""","SA_1.mp3"  ← CORRECT
```

### Issue 3: Line Breaks in Text
```csv
"P01","Group1",1,"SA","Text with
line break","SA_1.mp3"  ← WRONG (breaks parsing)
"P01","Group1",1,"SA","Text with line break","SA_1.mp3"  ← CORRECT
```

## Excel Export Tips

If editing in Excel:
1. Use "Save As" → CSV UTF-8
2. Excel automatically handles quotes
3. Verify in text editor after export
4. Check for extra blank lines at end

## Verification Checklist

- [ ] All text fields are in quotes
- [ ] Each line has exactly 6 columns
- [ ] Text with commas is properly quoted
- [ ] No line breaks inside text fields
- [ ] AudioFile column has filename or "NA"
- [ ] Console shows correct line count
- [ ] No "[CSV] Line X has only Y columns" warnings

## Still Having Issues?

Check Console for:
```
[CSV] Line 15 has only 4 columns, expected 6. Skipping.
```

This tells you which line has a problem. Open the CSV in a text editor and check line 15 (plus 1 for header = line 16 in the file).
